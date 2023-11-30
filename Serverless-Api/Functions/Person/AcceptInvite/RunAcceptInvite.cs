using Domain.Events;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Domain.Interfaces;

namespace Serverless_Api
{
    public partial class RunAcceptInvite
    {
        private readonly Person _user;
        private readonly IPersonRepository _peopleStore;
        private readonly IInvitesService _invitesService;

        public RunAcceptInvite(IPersonRepository peopleStore, Person user, IInvitesService invitesService)
        {
            _user = user;
            _peopleStore = peopleStore;
            _invitesService = invitesService;
        }

        [Function(nameof(RunAcceptInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/accept")] HttpRequestData req, string inviteId)
        {
            var answer = await req.Body<InviteAnswer>();
            var person = await _peopleStore.GetAsync(_user.Id);

            // caso o ultimo invite for com o status Accepted, impede que o usuario aceite para evitar repetições
            if (person.Invites.FirstOrDefault(e => e.Id == inviteId).Status == InviteStatus.Accepted)
            {
                return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
            }

            await _invitesService.AcceptBbqInvite(person, inviteId, answer.IsVeg);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}
