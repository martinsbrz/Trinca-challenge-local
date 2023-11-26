using Domain;
using Eveneum;
using CrossCutting;
using Domain.Events;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using static Domain.ServiceCollectionExtensions;

namespace Serverless_Api
{
    public partial class RunDeclineInvite
    {
        private readonly Person _user;
        private readonly IPersonRepository _repository;
        private readonly IBbqRepository _bbqRepository;

        public RunDeclineInvite(Person user, IPersonRepository repository, IBbqRepository bbqRepository)
        {
            _user = user;
            _repository = repository;
            _bbqRepository = bbqRepository;
        }

        [Function(nameof(RunDeclineInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/decline")] HttpRequestData req, string inviteId)
        {
            var person = await _repository.GetAsync(_user.Id);

            // caso o ultimo invite for com o status Accepted, impede que o usuario aceite para evitar repetições
            if (person.Invites.FirstOrDefault(e => e.Id == inviteId).Status == InviteStatus.Declined)
            {
                return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
            }

            if (person == null)
                return req.CreateResponse(System.Net.HttpStatusCode.NoContent);

            person.Apply(new InviteWasDeclined { InviteId = inviteId, PersonId = person.Id });

            await _repository.SaveAsync(person);
            //Implementar impacto da recusa do convite no churrasco caso ele já tivesse sido aceito antes

            var bbq = new Bbq();
            bbq = await _bbqRepository.GetAsync(inviteId);
            bbq.Apply(new InviteWasDeclined { InviteId = inviteId, PersonId = person.Id });
            await _bbqRepository.SaveAsync(bbq);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}
