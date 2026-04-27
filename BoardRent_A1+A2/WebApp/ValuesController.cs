using Microsoft.AspNetCore.Mvc;
using BoardRent.Repositories;
using BoardRent.Domain;

namespace BoardRent.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetById(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // GET: api/users/username/{username}
        [HttpGet("username/{username}")]
        public async Task<ActionResult<User>> GetByUsername(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // GET: api/users/email/{email}
        [HttpGet("email/{email}")]
        public async Task<ActionResult<User>> GetByEmail(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // GET: api/users?page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll(int page = 1, int pageSize = 10)
        {
            var users = await _userRepository.GetAllAsync(page, pageSize);
            return Ok(users);
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult> Create(User user)
        {
            await _userRepository.AddAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, User user)
        {
            if (id != user.Id)
                return BadRequest();

            await _userRepository.UpdateAsync(user);
            return NoContent();
        }

        // POST: api/users/{id}/roles
        [HttpPost("{id}/roles")]
        public async Task<ActionResult> AddRole(Guid id, [FromBody] string roleName)
        {
            await _userRepository.AddRoleAsync(id, roleName);
            return NoContent();
        }
    }
}