using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Notification;
using Vint.Core.ECS.Templates;
using Vint.Core.ECS.Templates.Notification;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User.PromoCode;

[ProtocolId(1490877430206)]
public class ActivatePromoCodeEvent : IServerEvent {
    public string Code { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        string[] parts = Code.Split('/');

        if (parts.Length > 2) return;

        IEntity item;

        try {
            item = GlobalEntities.GetEntity(parts[0], parts[1]);

            if (item.TemplateAccessor?.Template is not MarketEntityTemplate ||
                item.GetUserEntity(connection).HasComponent<UserGroupComponent>()) return;
        } catch (NullReferenceException) {
            return;
        }

        IEntity user = entities.Single();

        connection.PurchaseItem(item, 1, 0, false, false);
        connection.Share(new NewItemNotificationTemplate().Create(user, item, 1));
        connection.Send(new ShowNotificationGroupEvent(1), user);
    }
}