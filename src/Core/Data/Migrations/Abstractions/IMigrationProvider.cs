﻿using System.Collections.Generic;

namespace PlatoCore.Data.Migrations.Abstractions
{

    public interface IMigrationProvider
    {

        string ModuleId { get; }

        PreparedMigration GetSchema(string version);

        IList<PreparedMigration> Schemas { get; }

        IMigrationProvider LoadSchemas(IList<string> versions);

    }

}
