using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Domain.Events;

namespace Domain.Entities
{
    public class Bbq : AggregateRoot
    {
        public string Reason { get; set; }
        public BbqStatus Status { get; set; }
        public DateTime Date { get; set; }
        public bool IsTrincasPaying { get; set; }
        public IEnumerable<BbqShopList> ShopList { get; set; }

        public Bbq()
        {
            ShopList = new List<BbqShopList>();
        }

        public void When(ThereIsSomeoneElseInTheMood @event)
        {
            Id = @event.Id.ToString();
            Date = @event.Date;
            Reason = @event.Reason;
            Status = BbqStatus.New;
            IsTrincasPaying = @event.IsTrincasPaying;
        }

        public void When(BbqStatusUpdated @event)
        {
            if (@event.GonnaHappen)
                Status = BbqStatus.PendingConfirmations;
            else
                Status = BbqStatus.ItsNotGonnaHappen;

            if (@event.TrincaWillPay)
                IsTrincasPaying = true;
        }

        public void When(InviteWasAccepted @event)
        {
            var InviteList = ShopList.ToList();
            var invite = InviteList.FirstOrDefault(e => e.Id == @event.InviteId && e.PersonId == @event.PersonId);

            if (invite == null)
            {
                InviteList.Add(new BbqShopList
                {
                    Id = @event.InviteId,
                    PersonId = @event.PersonId,
                    Meat = @event.IsVeg ? 0 : 300,
                    Vegetables = @event.IsVeg ? 600 : 300,
                    Status = ShopListStatus.Will
                });
            }
            else
            {
                invite.Vegetables = @event.IsVeg ? 600 : 300;
                invite.Meat = @event.IsVeg ? 0 : 300;
                invite.Status = ShopListStatus.Will;
            }

            ShopList = InviteList;

            if (ShopList.Where(e => e.Status == ShopListStatus.Will).Count() >= 3 && Status != BbqStatus.Confirmed)
            {
                Status = BbqStatus.Confirmed;
            }
        }

        public void When(InviteWasDeclined @event)
        {
            //TODO:Deve ser possível rejeitar um convite já aceito antes.
            //Se este for o caso, a quantidade de comida calculada pelo aceite anterior do convite
            //deve ser retirado da lista de compras do churrasco.
            //Se ao rejeitar, o número de pessoas confirmadas no churrasco for menor que sete,
            //o churrasco deverá ter seu status atualizado para “Pendente de confirmações”.

            var InviteList = ShopList.ToList();
            var invite = InviteList.FirstOrDefault(e => e.Id == @event.InviteId && e.PersonId == @event.PersonId);

            if ( invite == null)
            {
                InviteList.Add(new BbqShopList
                {
                    Id = @event.InviteId,
                    PersonId = @event.PersonId,
                    Status = ShopListStatus.WillNot
                });
            }
            else
            {
                invite.Vegetables -= invite.Vegetables;       
                invite.Meat -= invite.Meat;
                invite.Status = ShopListStatus.WillNot;
            }

            ShopList = InviteList;
 
            if (ShopList.Where(e => e.Status == ShopListStatus.Will).Count() < 3 && Status != BbqStatus.PendingConfirmations)
            {
                Status = BbqStatus.PendingConfirmations;
            }
                
        }



        public object TakeSnapshot()
        {

            var a = ShopList.Sum(e => e.Vegetables);
            var b = ShopList.Sum(e => e.Meat);

            return new
            {
                Id,
                Date,
                IsTrincasPaying,
                Status = Status.ToString(),
                ShopList = ShopList.GroupBy(e => e.Id).Select(e => new { Vegetables = e.Sum(e => e.Vegetables), Meat = e.Sum(e => e.Meat) })
            };
        }
    }
}
