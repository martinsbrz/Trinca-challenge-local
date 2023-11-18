using System.Net;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunGetProposedBbqs
    {
        private readonly Person _user;
        private readonly IBbqRepository _bbqs;
        private readonly IPersonRepository _repository;
        public RunGetProposedBbqs(IPersonRepository repository, IBbqRepository bbqs, Person user)
        {
            _user = user;
            _bbqs = bbqs;
            _repository = repository;
        }

        [Function(nameof(RunGetProposedBbqs))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras")] HttpRequestData req)
        {
            var snapshots = new List<object>();
            var moderator = await _repository.GetAsync(_user.Id);
            foreach (var bbqId in moderator.Invites.Where(i => i.Date > DateTime.Now).Select(o => o.Id).ToList())
            {
                var bbq = await _bbqs.GetAsync(bbqId);
                snapshots.Add(bbq.TakeSnapshot());
            }

            return await req.CreateResponse(HttpStatusCode.Created, snapshots);
        }
    }
}
