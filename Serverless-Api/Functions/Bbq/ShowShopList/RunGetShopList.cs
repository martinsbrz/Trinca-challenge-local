using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Serverless_Api.RunAcceptInvite;
using Eveneum;
using CrossCutting;

namespace Serverless_Api.Functions.Bbq.ShowShopList
{
    public partial class RunGetShopList
    {
        public IBbqRepository _bbqRepository { get; set; }
        public IPersonRepository _personRepository { get; set; }
        public SnapshotStore  _snapshots { get; set; }
        public Person _user { get; set; }

        public RunGetShopList(IBbqRepository bbqRepository, Person user, SnapshotStore snapshots, IPersonRepository personRepository)
        {
            _bbqRepository = bbqRepository;
            _user = user;
            _snapshots = snapshots;
            _personRepository = personRepository;
        }

        [Function(nameof(RunGetShopList))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras/{inviteId}/shopList")] HttpRequestData req, string inviteId)
        {
            var person = await _personRepository.GetAsync(_user.Id);
            var bbq = await _bbqRepository.GetAsync(inviteId);

            if (bbq == null)
                return req.CreateResponse(System.Net.HttpStatusCode.NoContent);            

            if (!person.IsCoOwner)
            {
                return req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
            }

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, bbq.TakeSnapshot());
        }

    }
}
