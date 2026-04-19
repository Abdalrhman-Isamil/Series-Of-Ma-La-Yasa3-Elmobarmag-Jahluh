using JobListingsAPI.Filters;
using JobListingsAPI.Models;
using JobListingsAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobListingsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        // Controller depends only on IJobService — AppDbContext never injected directly
        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        // GET /api/jobs — returns only active listings
        [HttpGet]
        public IActionResult GetAll()
        {
            var jobs = _jobService.GetAllActive();
            return Ok(jobs);
        }

        // GET /api/jobs/{id} — returns a listing by ID regardless of soft-delete status
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var job = _jobService.GetById(id);
            if (job == null)
                return NotFound($"Job listing with ID {id} was not found.");
            return Ok(job);
        }

        // POST /api/jobs — creates a new listing
        // ValidateJobFilter runs BEFORE this action; invalid bodies never reach here
        [HttpPost]
        [ServiceFilter(typeof(ValidateJobFilter))]
        public IActionResult Create([FromBody] JobListing job)
        {
            _jobService.Create(job);
            return CreatedAtAction(nameof(GetById), new { id = job.Id }, job);
        }

        // PUT /api/jobs/{id} — updates an existing listing
        // ValidateJobFilter runs BEFORE this action; invalid bodies never reach here
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidateJobFilter))]
        public IActionResult Update(int id, [FromBody] JobListing job)
        {
            var existing = _jobService.GetById(id);
            if (existing == null)
                return NotFound($"Job listing with ID {id} was not found.");

            _jobService.Update(id, job);
            return NoContent();
        }

        // DELETE /api/jobs/{id} — soft delete (sets IsActive = false, record stays in DB)
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _jobService.GetById(id);
            if (existing == null)
                return NotFound($"Job listing with ID {id} was not found.");

            _jobService.SoftDelete(id);
            return NoContent();
        }
    }
}
