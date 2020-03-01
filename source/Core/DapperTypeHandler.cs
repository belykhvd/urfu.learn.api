using System;
using System.Data;
using Dapper;
using Newtonsoft.Json;

namespace Core
{
    public class DapperTypeHandler : SqlMapper.ITypeHandler
    {
        public object Parse(Type destinationType, object value)
        {
            return JsonConvert.DeserializeObject(value.ToString(), destinationType);
        }

        public void SetValue(IDbDataParameter parameter, object value)
        {
            parameter.Value = value == null ? (object) DBNull.Value : JsonConvert.SerializeObject(value);
            parameter.DbType = DbType.String;
        }
    }
}