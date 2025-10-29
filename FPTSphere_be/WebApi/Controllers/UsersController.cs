using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.User;
using BusinessLayer.Services.Interfaces;
using DataLayer.Data;
using DataLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class with the specified user service.
        /// </summary>
        /// <param name="userService">Service used to retrieve and manage user data.</param>
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a 200 OK response with the list of users.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        /// <summary>
        /// Retrieves the user with the specified id.
        /// </summary>
        /// <param name="id">The identifier of the user to retrieve.</param>
        /// <returns>200 OK with the user when found; 404 NotFound if no user exists with the given id.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Creates a new user from the provided DTO and returns the created resource.
        /// </summary>
        /// <param name="dto">Data transfer object containing information required to create the user.</param>
        /// <returns>
        /// 201 Created with the created user in the response body and a Location header pointing to the GetById route;
        /// 400 Bad Request with model state details if the request model is invalid.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _userService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.UserId }, created);
        }

        /// <summary>
        /// Updates an existing user identified by <paramref name="id"/> using the values in <paramref name="dto"/>.
        /// </summary>
        /// <param name="id">The identifier of the user to update.</param>
        /// <param name="dto">An UpdateUserDto containing the user's identifier and updated values.</param>
        /// <returns>
        /// 204 NoContent when the update succeeds; 400 BadRequest with message "ID mismatch" when <paramref name="id"/> does not match <paramref name="dto"/>.UserId; 404 NotFound if the target user does not exist or the update could not be applied.
        /// </returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            if (id != dto.UserId) return BadRequest("ID mismatch");
            var updated = await _userService.UpdateAsync(dto);
            if (!updated) return NotFound();
            return NoContent();
        }
    }
}