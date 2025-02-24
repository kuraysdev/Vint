﻿using System.Net.Mail;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(635906273125139964)]
public class CheckEmailEvent : IServerEvent {
    public string Email { get; private set; } = null!;
    public bool IncludeUnconfirmed { get; private set; } //todo

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        try {
            MailAddress email = new(Email);

            using DbConnection db = new();

            if (db.Players.Any(player => player.Email == email.Address))
                connection.Send(new EmailOccupiedEvent(Email));
            else connection.Send(new EmailVacantEvent(Email));
        } catch {
            connection.Send(new EmailInvalidEvent(Email));
        }
    }
}