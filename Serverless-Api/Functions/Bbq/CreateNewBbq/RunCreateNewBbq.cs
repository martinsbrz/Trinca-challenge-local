using Eveneum;
using System.Net;
using CrossCutting;
using Domain.Events;
using Domain.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Domain.Repositories;
using Domain.Interfaces;

namespace Serverless_Api
{
    public partial class RunCreateNewBbq
    {
        private readonly Person _user;        
        private readonly IBbqService _bbqService;
        private readonly IInvitesService _invitesService;



        public RunCreateNewBbq(Person user, IBbqService bbqService, IInvitesService invitesService)
        {
            _user = user;            
            _bbqService = bbqService;
            _invitesService = invitesService;
        }

        [Function(nameof(RunCreateNewBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "churras")] HttpRequestData req)
        {
            var input = await req.Body<NewBbqRequest>();

            if (input == null)
            {
                return await req.CreateResponse(HttpStatusCode.BadRequest, "input is required.");
            }

            var churras = await _bbqService.ThereIsSomeoneElseInTheMood(input.Date, input.Reason, input.IsTrincasPaying);   
            
            churras.

            await _invitesService.SendNewBbq(churras!.Id, churras.Date, churras.Reason);

            var churrasSnapshot = churras.TakeSnapshot();

            return await req.CreateResponse(HttpStatusCode.Created, churrasSnapshot);
        }
    }
}
