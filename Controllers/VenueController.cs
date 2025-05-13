/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class VenueController : Controller
    {
        private readonly POEDBContext _context;

        public VenueController(POEDBContext context)
        {
            _context = context;
        }

        // GET: Venue
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venues.ToListAsync());
        }

        // GET: Venue/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venue/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venue/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,VenueName,Location,Capacity,ImageUrl")] Venue venue)
        {
            if (ModelState.IsValid)
            {
                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venue/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: Venue/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageUrl")] Venue venue)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venue/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: Venue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue != null)
            {
                _context.Venues.Remove(venue);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Azure.Storage;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using EventEase.Data;
using EventEase.Models;
using System.IO; // Needed for Path

namespace EventEase.Controllers
{
    public class VenueController : Controller
    {
        private readonly POEDBContext _context;
        private readonly string storageAccountName;
        private readonly string storageAccountKey;
        private readonly string containerName;

        public VenueController(POEDBContext context, IConfiguration config)
        {
            _context = context;
            storageAccountName = config["AzureBlob:storageAccountName"] ?? throw new ArgumentNullException("AzureBlob:storageAccountName");
            storageAccountKey = config["AzureBlob:storageAccountKey"] ?? throw new ArgumentNullException("AzureBlob:storageAccountKey");
            containerName = config["AzureBlob:containername"] ?? "venue-image";
        }

        // Set up blob container client
        private BlobContainerClient GetContainerClient()
        {
            var serviceUri = new Uri($"https://{storageAccountName}.blob.core.windows.net");
            var serviceClient = new BlobServiceClient(serviceUri, new StorageSharedKeyCredential(storageAccountName, storageAccountKey));
            return serviceClient.GetBlobContainerClient(containerName);
        }

        // Set up image upload method
        private async Task<string> UploadImageToBlobAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("No file provided for upload.");
                return null;
            }

            try
            {
                var containerClient = GetContainerClient();
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

                var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = containerClient.GetBlobClient(blobName);

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

                Console.WriteLine($"Uploaded blob: {blobName}");
                return blobName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading to Blob Storage: {ex.Message}");
                return null;
            }
        }

        // Set up Shared Access Signature (SAS) token
        private string GenerateSASurl(string blobName)
        {
            if (string.IsNullOrEmpty(blobName))
            {
                throw new ArgumentNullException(nameof(blobName), "Blob name cannot be null or empty.");
            }

            var blobUri = new Uri($"https://{storageAccountName}.blob.core.windows.net/{containerName}/{blobName}");
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(storageAccountName, storageAccountKey)).ToString();
            return $"{blobUri}?{sasToken}";
        }

        // GET: Venue
        public async Task<IActionResult> Index(string searchQuery)
        {
            var venues = _context.Venues.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                venues = venues.Where(v =>
                    v.VenueId.ToString().Contains(searchQuery) ||
                    v.VenueName.Contains(searchQuery) ||
                    v.Location.Contains(searchQuery));
            }

            return View(await venues.ToListAsync());
        }

        // GET: Venue/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venue/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venue/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,VenueName,Location,Capacity")] Venue venue, IFormFile ImageUrl)
        {
            Console.WriteLine($"ImageUrl received: {(ImageUrl != null ? ImageUrl.FileName : "null")}");
            ModelState.Remove("ImageUrl");

            if (ImageUrl == null || ImageUrl.Length == 0)
            {
                ModelState.AddModelError("ImageUrl", "An image is required.");
                Console.WriteLine("Validation failed: No image provided.");
                return View(venue);
            }

            var blobName = await UploadImageToBlobAsync(ImageUrl);
            if (blobName == null)
            {
                ModelState.AddModelError("ImageUrl", "Failed to upload image to Azure Blob Storage.");
                Console.WriteLine("Validation failed: Image upload failed.");
                return View(venue);
            }

            var venueWithLinks = new Venue
            {
                VenueName = venue.VenueName,
                Location = venue.Location,
                Capacity = venue.Capacity,
                ImageUrl = GenerateSASurl(blobName)
            };

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine("ModelState Errors: " + string.Join(", ", errors));
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("Saving venue to database...");
                _context.Add(venueWithLinks);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Venue {venueWithLinks.VenueName} {venueWithLinks.Location}was added successfully";
                return RedirectToAction(nameof(Index));

            }

            Console.WriteLine("Returning view due to invalid ModelState.");

            
            return View(venue);
        }

        // GET: Venue/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: Venue/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageUrl")] Venue venue, IFormFile ImageUrl)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            ModelState.Remove("ImageUrl");
            Console.WriteLine($"ImageUrl received: {(ImageUrl != null ? ImageUrl.FileName : "null")}");

            if (ImageUrl != null && ImageUrl.Length > 0)
            {
                var blobName = await UploadImageToBlobAsync(ImageUrl);
                if (blobName == null)
                {
                    ModelState.AddModelError("ImageUrl", "Failed to upload image to Azure Blob Storage.");
                    Console.WriteLine("Validation failed: Image upload failed.");
                    return View(venue);
                }
                venue.ImageUrl = GenerateSASurl(blobName);
            }
            else if (string.IsNullOrEmpty(venue.ImageUrl))
            {
                ModelState.AddModelError("ImageUrl", "An image is required.");
                Console.WriteLine("Validation failed: No image provided and existing ImageUrl is empty.");
                return View(venue);
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine("ModelState Errors: " + string.Join(", ", errors));
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("Updating venue in database...");
                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("Returning view due to invalid ModelState.");
            return View(venue);
        }

        // GET: Venue/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: Venue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.Include(b => b.Bookings).FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }
            if (venue.Bookings.Any())
            {
                TempData["Error"] = "Cannot delete venue";
                return RedirectToAction(nameof(Index));
            }
            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Venue deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}