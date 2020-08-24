using Microsoft.AspNetCore.Mvc;

namespace PalTracker
{
    [Route("/")]
    public class WelcomeController : ControllerBase
    {
        public readonly WelcomeMessage _message;

        public WelcomeController(WelcomeMessage mess){
            this._message = mess;

        }
        [HttpGet]
        public string SayHello() => this._message.Message;
    }
}