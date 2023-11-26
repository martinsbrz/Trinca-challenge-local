using Domain.Events;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunAcceptInvite
    {
        private readonly Person _user;
        private readonly IPersonRepository _repository;
        private readonly IBbqRepository _bbqRepository;
        public RunAcceptInvite(IPersonRepository repository, Person user, IBbqRepository bbqRepository)
        {
            _user = user;
            _repository = repository;
            _bbqRepository = bbqRepository;
        }

        [Function(nameof(RunAcceptInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/accept")] HttpRequestData req, string inviteId)
        {
            var answer = await req.Body<InviteAnswer>();
            var person = await _repository.GetAsync(_user.Id);

            // caso o ultimo invite for com o status Accepted, impede que o usuario aceite para evitar repetições
            if (person.Invites.FirstOrDefault(e => e.Id == inviteId).Status == InviteStatus.Accepted)
            {
                return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
            }

            person.Apply(new InviteWasAccepted { InviteId = inviteId, IsVeg = answer.IsVeg, PersonId = person.Id });
            await _repository.SaveAsync(person);

            //implementar efeito do aceite do convite no churrasco
            //quando tiver 7 pessoas ele está confirmado
            var bbq = new Bbq();
            bbq = await _bbqRepository.GetAsync(inviteId);
            bbq.Apply(new InviteWasAccepted { InviteId = inviteId, IsVeg = answer.IsVeg, PersonId = person.Id });
            await _bbqRepository.SaveAsync(bbq);

            

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}
