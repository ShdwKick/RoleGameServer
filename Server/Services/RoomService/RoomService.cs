using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Server.Data;

namespace GraphQLServer.Services.RoomService;

public class RoomService : IRoomService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;
    private readonly DataBaseConnection _dataBaseConnection;


    public RoomService(IHttpContextAccessor httpContextAccessor, IMemoryCache cache,
        IHttpClientFactory httpClientFactory, DataBaseConnection dataBaseConnection)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
        _dataBaseConnection = dataBaseConnection;
    }

    
}