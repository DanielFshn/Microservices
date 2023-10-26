using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataClient.Http;

namespace PlatformService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlatformsController : ControllerBase
{
    private readonly IPlatformRepo _platformRepo;
    private readonly IMapper _mapper;
    private readonly ICommandDataClient _commandDataClient;

    public PlatformsController(IPlatformRepo platformRepo, IMapper mapper, ICommandDataClient commandDataClient) =>
        (_platformRepo, _mapper, _commandDataClient) = (platformRepo, mapper, commandDataClient);

    [HttpGet]
    public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
    {
        var platforms = _platformRepo.GetAllPlatforms();
        return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
    }

    [HttpGet("{id}", Name = "GetPlatformById")]
    public ActionResult<PlatformReadDto> GetPlatformById(int id)
    {
        var platformMayBe = _platformRepo.GetPlatformById(id);

        if (platformMayBe != null)
            return Ok(_mapper.Map<PlatformReadDto>(platformMayBe));
        return NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto request)
    {
        var platformModel = _mapper.Map<Platform>(request);
        _platformRepo.CreatePlatform(platformModel);
        _platformRepo.SaveChanges();
        var platformReadDto = _mapper.Map<PlatformReadDto>(platformModel);
        try
        {
            await _commandDataClient.SendPlatformToCommand(platformReadDto);
        }
        catch (Exception e)
        {
            Console.WriteLine($"--> Could not sent synchronously: {e.Message}");
        }


        //try
        //{
        //    var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(platformReadDto);
        //    platformPublishedDto.Event = "Platform_Published";
        //    _messageBusClient.PublishNewPlatform(platformPublishedDto);
        //}
        //catch (Exception e)
        //{
        //    Console.WriteLine($"--> Could not sent asynchronously: {e.Message}");
        //}

        return CreatedAtRoute(nameof(GetPlatformById), new { id = platformReadDto.Id }, platformReadDto);
    }
}