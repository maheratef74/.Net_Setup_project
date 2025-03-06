using App.Application.Services.ResponseService;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace App.API.Controllers;

    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IResponseService _responseService;
        public AuthenticationController(IMediator mediator 
            , IResponseService responseService)
        {
            _mediator = mediator;
            _responseService = responseService;
        }
     
        
    }
