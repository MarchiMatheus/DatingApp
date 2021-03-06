using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dto;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public UsersController(IDatingRepository repo,
                               IMapper mapper)
        {
            this._repo = repo;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await this._repo.GetUser(currentUserId);

            userParams.UserId = currentUserId;
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = (userFromRepo.Gender == "male")? "female" : "male";
            }

            var users = await this._repo.GetUsers(userParams);
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await this._repo.GetUser(id);
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);

            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await this._repo.GetUser(id);

            this._mapper.Map(userForUpdateDto, userFromRepo);

            if (await this._repo.SaveAll())
                return NoContent();

            throw new Exception($"Updating user with {id} failed on save");
        }

        [HttpPost("{id}/like/{recepientId}")]
        public async Task<IActionResult> LikeUser(int id, int recepientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var like = await this._repo.GetLike(id, recepientId);

            if (like != null)
                return BadRequest("You already liked this user");

            if (await this._repo.GetUser(recepientId) == null)
                return NotFound();

            like = new Like
            {
                LikerId = id,
                LikeeId = recepientId
            };

            this._repo.Add<Like>(like);

            if(await this._repo.SaveAll())
                return Ok();
            else
                return BadRequest("Failed to like user");
        }
    }
}