using Domain.Entities;
using Domain.Events;
using Domain.Interfaces;
using Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;



namespace Domain.Services
{
    internal class BbqService : IBbqService
    {
        private readonly IBbqRepository _bbqsStore;
        private readonly IPersonRepository _peopleStore;

        public BbqService(IBbqRepository bbqsStore, IPersonRepository peopleStore)
        {
            _bbqsStore = bbqsStore;
            _peopleStore = peopleStore;
        }

        public async Task<Bbq?> BbqStatusUpdated(Bbq churras, bool GonnaHappen, bool TrincaWillPay)
        {
            churras.Apply(new BbqStatusUpdated(GonnaHappen, TrincaWillPay));

            await _bbqsStore.SaveAsync(churras);

            return churras;
        }

        public async Task<Bbq?> ThereIsSomeoneElseInTheMood(DateTime Date, string Reason, bool IsTrincasPaying)
        {
            var churras = new Bbq();
            churras.Apply(new ThereIsSomeoneElseInTheMood(Guid.NewGuid(), Date, Reason, IsTrincasPaying));
            await _bbqsStore.SaveAsync(churras);            

            return churras;
        }

        public async Task<List<object>> GetProposedBbqs(string UserId)
        {
            var snapshots = new List<object>();
            var moderator = await _peopleStore.GetAsync(UserId);

            foreach (var bbqId in moderator.Invites.Where(i => i.Date > DateTime.Now).Select(o => o.Id).ToList())
            {
                var bbq = await _bbqsStore.GetAsync(bbqId);

                if (bbq.Status != BbqStatus.ItsNotGonnaHappen)
                {
                    snapshots.Add(bbq.TakeSnapshot());
                }
            }

            return snapshots;
        }

        public async Task<HttpStatusCode> GetShopList(Bbq? Churras, Person Person)
        {
            if (Churras == null)
                return HttpStatusCode.NoContent;

            if (!Person.IsCoOwner)
                return HttpStatusCode.Unauthorized;

            return HttpStatusCode.OK;

        }
    }
}
