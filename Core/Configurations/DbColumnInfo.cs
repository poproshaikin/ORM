﻿using ORM.Core.Enums;

namespace ORM.Core.Configurations;

internal class DbColumnInfo
{
    public int Cid { get; set; }
    public string Name { get; set; }
    public DbConstraint[] Constraints { get; set; }
    public string TableName { get; set; }
}