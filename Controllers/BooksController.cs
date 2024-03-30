using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace LibraryApi.Controllers
{
    public class BooksController : ControllerBase
    {
        private readonly Models.LibraryApiContext _context;

        public BooksController(Models.LibraryApiContext context)
        {
            _context = context;
        }

        // GET: api/Books
        [HttpGet("api/Books")]
        [Authorize]
        public async Task<ActionResult<List<Book>>> GetBooks()
        {
            return await _context.Books.ToListAsync();
        }

        // GET: api/Books/5
        [HttpGet("api/Books/{id}")]
        [Authorize]
        public async Task<ActionResult<Book>> GetBook(Guid id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }

        // POST: api/Books
        [HttpPost("api/Books/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<Book>> CreateBook([FromBody] Book book)
        {
            if (ModelState.IsValid)
            {
                // Genera un nuevo GUID para el Id
                book.Id = Guid.NewGuid();
                _context.Add(book);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
            }

            return BadRequest(ModelState);
        }

        // PUT: api/Books/5
        [HttpPut("api/Books/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateBook(Guid id, [FromBody] Book book)
        {
            if (id != book.Id)
            {
                return BadRequest("El ID del libro en la URL no coincide con el ID del libro enviado en el cuerpo de la solicitud.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
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

        // DELETE: api/Books/5
        [HttpDelete("api/Books/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteBook(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Books/search?query=
        [HttpGet("api/Books/search")]
        [Authorize]
        public async Task<ActionResult<List<SelectListItem>>> SearchBooks(string query)
        {
            var books = await _context.Books.Where(b => b.Title.Contains(query)).ToListAsync();
            var options = books.Select(b => new SelectListItem { Text = b.Title, Value = b.Id.ToString() });
            return Ok(options);
        }

        private bool BookExists(Guid id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}

