using System.Net;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunGetProposedBbqs
    {
        private readonly Person _user;
        private readonly IBbqService _bbqService;
        public RunGetProposedBbqs(IPersonRepository repository, IBbqRepository bbqs, Person user, IBbqService bbqService)
        {
            _user = user;
            _bbqService = bbqService;
        }

        [Function(nameof(RunGetProposedBbqs))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras")] HttpRequestData req)
        {

            var snapshots = await _bbqService.GetProposedBbqs(_user.Id);

            return await req.CreateResponse(HttpStatusCode.Created, snapshots);
        }
    }
}
