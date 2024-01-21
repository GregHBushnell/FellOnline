﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FellOnline.Database.Npgsql.Entities
{
    [Table("accounts", Schema = "fell_online_postgresql")]
    public class AccountEntity
    {
        [Key]
        public string Name { get; set; }
        public string Salt { get; set; }
        public string Verifier { get; set; }
        public byte AccessLevel { get; set; }
        public DateTime Created { get; set; }
        public DateTime Lastlogin { get; set; }
    }
}