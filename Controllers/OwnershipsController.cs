using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace LibraryApi.Controllers
{
    public class OwnershipsController : ControllerBase
    {
        private readonly Models.LibraryApiContext _context;

        public OwnershipsController(Models.LibraryApiContext context)
        {
            _context = context;
        }

        // GET: api/Ownerships
        [HttpGet("api/Ownerships")]
        [Authorize]
        public async Task<ActionResult<List<Ownership>>> GetOwnerships()
        {
            return await _context.Ownerships.Include(o => o.Book).Include(o => o.User).ToListAsync();
        }

        // GET: api/Ownerships/5
        [HttpGet("api/Ownerships/{id}")]
        [Authorize]
        public async Task<ActionResult<Ownership>> GetOwnership(Guid id)
        {
            var ownership = await _context.Ownerships
                .Include(o => o.Book)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ownership == null)
            {
                return NotFound();
            }

            return ownership;
        }

        // POST: api/Ownerships
        [HttpPost("api/Ownerships/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<Ownership>> CreateOwnership([FromBody] Ownership ownership)
        {
            if (ModelState.IsValid)
            {
                ownership.Id = Guid.NewGuid(); // Genera un nuevo GUID para el Id
                _context.Add(ownership);
                try
                {
                    await _context.SaveChangesAsync();
                    return CreatedAtAction(nameof(GetOwnership), new { id = ownership.Id }, ownership);
                }
                catch (DbUpdateException ex) // Manejar las excepciones de integridad aquí
                {
                    ModelState.AddModelError(string.Empty, "Error al guardar el ownership.");
                }
            }
            return BadRequest(ModelState);
        }

        // PUT: api/Ownerships/5
        [HttpPut("api/Ownerships/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateOwnership(Guid id, [FromBody] Ownership ownership)
        {
            if (id != ownership.Id)
            {
                return BadRequest("El ID del ownership en la URL no coincide con el ID del ownership enviado en el cuerpo de la solicitud.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ownership);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OwnershipExists(ownership.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return NoContent();
            }
            return BadRequest(ModelState);
        }

        // DELETE: api/Ownerships/5
        [HttpDelete("api/Ownerships/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteOwnership(Guid id)
        {
            var ownership = await _context.Ownerships.FindAsync(id);
            if (ownership == null)
            {
                return NotFound();
            }

            _context.Ownerships.Remove(ownership);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OwnershipExists(Guid id)
        {
            return _context.Ownerships.Any(e => e.Id == id);
        }
    }
}

