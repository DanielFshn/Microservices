using PlatformService.Dtos;

namespace PlatformService.SyncDataClient.Http;

public interface ICommandDataClient
{
    Task SendPlatformToCommand(PlatformReadDto plat);
}
