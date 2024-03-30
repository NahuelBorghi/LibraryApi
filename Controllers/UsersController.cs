using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Models;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;

namespace LibraryApi.Controllers
{
    public class UsersController : ControllerBase
    {
        private readonly Models.LibraryApiContext _context;

        public UsersController(Models.LibraryApiContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet("api/Users")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("api/Users/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/Users
        [HttpPost("api/Users")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                // Genera un nuevo GUID para el Id
                user.Id = Guid.NewGuid();
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                _context.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }

            return BadRequest(ModelState);
        }

        // PUT: api/Users/5
        [HttpPut("api/Users/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] User user)
        {
            if (id != user.Id)
            {
                return BadRequest("El ID del usuario en la URL no coincide con el ID del usuario enviado en el cuerpo de la solicitud.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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

        // DELETE: api/Users/5
        [HttpDelete("api/Users/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Users/search?query=
        [HttpGet("api/Users/search")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<List<SelectListItem>>> SearchUsers(string query)
        {
            var users = await _context.Users.Where(u => u.UserName.Contains(query)).ToListAsync();
            var options = users.Select(u => new SelectListItem { Text = u.UserName, Value = u.Id.ToString() });
            return Ok(options);
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
