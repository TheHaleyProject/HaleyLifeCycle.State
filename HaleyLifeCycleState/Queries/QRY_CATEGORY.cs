using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Haley.Internal.QueryFields;

namespace Haley.Internal {
    internal static class QRY_CATEGORY {
        public const string INSERT = $@"INSERT INTO category (display_name) VALUES ({DISPLAY_NAME}); SELECT LAST_INSERT_ID();";
        public const string GET_ALL = $@"SELECT id, display_name, name FROM category ORDER BY display_name;";
        public const string GET_BY_NAME = $@"SELECT id, display_name, name FROM category WHERE name = lower({NAME});";
        public const string GET_BY_ID = $@"SELECT id, display_name, name FROM category WHERE id = {ID};";
        public const string DELETE_BY_ID = $@"DELETE FROM category WHERE id = {ID};";
        public const string EXISTS = $@"SELECT COUNT(*) FROM category WHERE name = lower({NAME});";
        //public const string SEED = $@"INSERT INTO category (display_name) VALUES ('System'), ('Business'), ('Technical'), ('Error');";
    }
}
