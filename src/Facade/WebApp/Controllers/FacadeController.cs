using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApp.Controllers
{
    using System.Threading.Tasks;
    using Configuration;
    using DTO;
    using IO.Ably;
    using Topic.Contract;

    [ApiController]
    [Route("[controller]")]
    public class FacadeController : ControllerBase
    {
        private readonly ILogger<FacadeController> _logger;
        private readonly AblyRealtime _ably;
        private readonly AblyConfig _ablyConfig;

        public FacadeController(ILogger<FacadeController> logger, AblyRealtime ably, AblyConfig ablyConfig)
        {
            _logger = logger;
            _ably = ably;
            _ablyConfig = ablyConfig;
        }

        [HttpPost]
        public async Task<StatusCodeResult> Save([FromBody] CreateDto create)
        {
            var data = new Topic
            (
                name: Name.NewName("name"),
                id: Identifier.NewId(""),
                creator: Creator.NewCreator(create.Creator),
                description: Description.NewDescription(create.Description),
                done: Done.NewDone(false)
            );

            var channel = _ably.Channels.Get(_ablyConfig.Topic.Name);
            var result = await channel.PublishAsync(_ablyConfig.Topic.MessageType, data);
            if (!result.IsFailure) 
                return Ok();
            _logger.LogError(result.Error.Message);
            return new StatusCodeResult(500);
        }
        
        [HttpPut]
        public async Task<StatusCodeResult> Update([FromBody] CreateDto create)
        {
            var data = new Topic
            (
                name: Name.NewName("name"),
                id: Identifier.NewId(create.Id),
                creator: Creator.NewCreator(create.Creator),
                description: Description.NewDescription(create.Description),
                done: Done.NewDone(true)
            );

            var channel = _ably.Channels.Get(_ablyConfig.Topic.Name);
            var result = await channel.PublishAsync(_ablyConfig.Topic.MessageType, data);
            if (!result.IsFailure) 
                return Ok();
            _logger.LogError(result.Error.Message);
            return new StatusCodeResult(500);
        }

        [HttpGet]
        public OkObjectResult Configuration() =>
            Ok(new {apiKey = _ablyConfig.ApiKey, push = _ablyConfig.Push});
    }
}