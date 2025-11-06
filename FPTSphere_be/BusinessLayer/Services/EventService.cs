using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.Event;
using BusinessLayer.Services.Interfaces;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _repo;
        private readonly IMapper _mapper;

        public EventService(IEventRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        public async Task<EventDto> CreateAsync(CreateEventDto dto)
        {
            var entity = _mapper.Map<Event>(dto);
            var created = await _repo.CreateAsync(entity);
            return _mapper.Map<EventDto>(created);
        }
        public async Task<List<EventDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<List<EventDto>>(list);
        }
        public async Task<EventDto?> GetByIdAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id);
            return e == null ? null : _mapper.Map<EventDto>(e);
        }
    }
}
