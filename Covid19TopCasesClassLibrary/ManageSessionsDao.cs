using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using Dapper;
using NLog;
using ScanChekServer.Models;

namespace ScanChekServer.Dao
{
    /// <summary>
    /// Clase donde se administran sesiones de Usuarios en tabla de servidor (ORACLE/SQL)
    /// </summary>
    public class ManageSessionsDao
    {
        private static ILogger Logger => LogManager.GetCurrentClassLogger();

        public static bool InsertUpdateUser(string _tkn, string _user, string _hostname, string _ipMaq, DateTime _hora)
        {
            Logger.Info("[CheckScanServer] Se consume servicio InsertUpdateUser");
            var response = false;
            try
            {
                var encontrado = EncontrarUsuario(_hostname, _ipMaq);
                //si el usuario ya esta registrado en tabla
                if (encontrado)
                {
                    Logger.Info("[CheckScanServer] UpdateUser: actualizando usuario {User} token:{Token} host:{Host} ip:{Ip}", _user, _tkn, _hostname, _ipMaq);

                    var update = $@"UPDATE {LocalSettings.schema}.SESIONES SET HORA=@hora, IP_MAQ=@ipmaq,TOKEN=@token WHERE NOMBRE_MAQ=@nombremaq";
                    long id = 0;
                    if (LocalSettings.dbotipo.Equals("SQL", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //SQL SERVER
                        Logger.Info("[CheckScanServer] Conexion a base de datos SQL SERVER");
                        using (var connection = new SqlConnection(LocalSettings.connectionString))
                        {
                            connection.Open();
                            id = connection.Execute(update, new { hora = _hora, ipmaq = _ipMaq, token = _tkn, nombremaq = _hostname });
                            connection.Close();
                        }
                    }
                    else
                    {
                        //ORACLE
                        update = LocalSettings.GetOracleQueryFormat(update, "@");
                        using (var connection = LocalSettings.GetOracleConnectionType())
                        {
                            connection.Open();
                            id = connection.Execute(update, new { hora = _hora, ipmaq = _ipMaq, token = _tkn, nombremaq = _hostname });
                            connection.Close();
                        }
                    }
                    response = true;
                    Logger.Info("[CheckScanServer] UpdateUser -> Usuario actualizado con exito. Username: {User} Id: {id}", _user, id);
                }
                else //agregar nuevo usuario a dbo.Sesiones
                {
                    Logger.Info("[CheckScanServer] InsertUsuario: usuario nuevo {User} token:{Token} host:{Host} ip:{Ip}", _user, _tkn, _hostname, _ipMaq);
                    long id;
                    if (LocalSettings.dbotipo.Equals("SQL", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //SQL SERVER
                        Logger.Info("[CheckScanServer] Conexion a base de datos SQL SERVER");
                        var insert = $@"INSERT INTO {LocalSettings.schema}.SESIONES 
                            (TOKEN, USERNAME, NOMBRE_MAQ, IP_MAQ,HORA) values (@token,@username,@nombremaq,@ipmaq,@hora);
                            SELECT CAST(SCOPE_IDENTITY() as bigint)  ";

                        using (var connection = new SqlConnection(LocalSettings.connectionString))
                        {
                            connection.Open();
                            var param = new DynamicParameters();
                            param.Add(name: "token", value: _tkn, direction: ParameterDirection.Input);
                            param.Add(name: "username", value: _user, direction: ParameterDirection.Input);
                            param.Add(name: "nombremaq", value: _hostname, direction: ParameterDirection.Input);
                            param.Add(name: "ipmaq", value: _ipMaq, direction: ParameterDirection.Input);
                            param.Add(name: "hora", value: _hora, direction: ParameterDirection.Input);

                            id = connection.Query<long>(insert, param).Single();
                            //id = param.Get<long>("Id");
                            connection.Close();
                        }
                    }
                    else
                    {
                        //ORACLE
                        var insert = $@"INSERT INTO {LocalSettings.schema}.SESIONES (TOKEN, USERNAME, NOMBRE_MAQ, IP_MAQ,HORA) values (:token,:username,:nombremaq,:ipmaq,:hora)  returning ID_SESION into :Id";
                        insert = LocalSettings.GetOracleQueryFormat(insert, ":");

                        using (var connection = LocalSettings.GetOracleConnectionType())
                        {
                            connection.Open();
                            var param = new DynamicParameters();
                            param.Add(name: "token", value: _tkn, direction: ParameterDirection.Input);
                            param.Add(name: "username", value: _user, direction: ParameterDirection.Input);
                            param.Add(name: "nombremaq", value: _hostname, direction: ParameterDirection.Input);
                            param.Add(name: "ipmaq", value: _ipMaq, direction: ParameterDirection.Input);
                            param.Add(name: "hora", value: _hora, direction: ParameterDirection.Input);
                            //Se ocupa DbType.String y size definido para parametro de salida debido a que ODBC no reconoce DbType.Int64
                            //param.Add(name: "Id", dbType: DbType.Int64, direction: ParameterDirection.Output);
                            param.Add(name: "Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 30);

                            connection.Execute(insert, param);
                            //id = param.Get<long>("Id");
                            id = long.Parse(param.Get<string>("Id"));
                            connection.Close();
                        }
                    }
                    response = true;
                    Logger.Info("[CheckScanServer] InsertUser -> Usuario nuevo insertado con exito username: {User} id: {id}", _user, id);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[CheckScanServer] InsertUpdateUser: Error al actualizar datos del usuario: {User}", _user);
            }
            return response;
        }

        public static void BorrarUsuario(string host, string ip)
        {
            try
            {
                Logger.Info("[CheckScanServer] Se consume servicio BorrarUsuario con host: {Host} ip: {Ip}", host, ip);
                var delete = $"DELETE FROM {LocalSettings.schema}.SESIONES WHERE NOMBRE_MAQ=@host AND IP_MAQ=@ip";
                if (LocalSettings.dbotipo.Equals("SQL", StringComparison.InvariantCultureIgnoreCase))
                {
                    //SQL SERVER
                    Logger.Info("[CheckScanServer] Conexion a base de datos SQL SERVER");
                    using (var connection = new SqlConnection(LocalSettings.connectionString))
                    {
                        connection.Open();
                        connection.Execute(delete, new { host, ip });
                        connection.Close();
                    }
                }
                else
                {
                    //ORACLE
                    delete = LocalSettings.GetOracleQueryFormat(delete, "@");
                    using (var connection = LocalSettings.GetOracleConnectionType())
                    {
                        connection.Open();
                        connection.Execute(delete, new { host, ip });
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[CheckScanServer] BorrarUsuario: Error al borrar usuario por host: {Host} ip: {Ip}", host, ip);
            }
        }

        public static int BorrarUsuario(int id)
        {
            Logger.Info("[CheckScanServer] Se consume servicio BorrarUsuario con id: {UserId}", id);
            var response = -1;
            try
            {
                var delete = $"DELETE FROM {LocalSettings.schema}.SESIONES WHERE ID_SESION=@idsesion";
                if (LocalSettings.dbotipo.Equals("SQL", StringComparison.InvariantCultureIgnoreCase))
                {
                    //SQL SERVER
                    Logger.Info("[CheckScanServer] Conexion a base de datos SQL SERVER");
                    using (var connection = new SqlConnection(LocalSettings.connectionString))
                    {
                        connection.Open();
                        response = connection.Execute(delete, new { idsesion = id });
                        connection.Close();
                    }
                }
                else
                {
                    //ORACLE
                    delete = LocalSettings.GetOracleQueryFormat(delete, "@");
                    using (var connection = LocalSettings.GetOracleConnectionType())
                    {
                        connection.Open();
                        response = connection.Execute(delete, new { idsesion = id });
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Write(ex);
                Logger.Error(ex, "[CheckScanServer] BorrarUsuario(id): Error al borrar usuario {UserId}", id);
            }
            return response;
        }

        //contar usuarios conectados
        public static long ContarUsers()
        {
            Logger.Info("[CheckScanServer] Se consume servicio ContarUsers");
            long usuariosConectados = 0;
            try
            {
                var countSQL = $"SELECT COUNT(*) FROM {LocalSettings.schema}.SESIONES";
                if (LocalSettings.dbotipo.Equals("SQL", StringComparison.InvariantCultureIgnoreCase))
                {
                    //SQL SERVER
                    Logger.Info("[CheckScanServer] Conexion a base de datos SQL SERVER");
                    using (var connection = new SqlConnection(LocalSettings.connectionString))
                    {
                        connection.Open();
                        usuariosConectados = connection.ExecuteScalar<int>(countSQL);
                        connection.Close();
                    }
                }
                else
                {
                    //ORACLE
                    using (var connection = LocalSettings.GetOracleConnectionType())
                    {
                        connection.Open();
                        usuariosConectados = connection.ExecuteScalar<int>(countSQL);
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                usuariosConectados = -1;
                Logger.Error(ex, "[CheckScanServer] ContarUsers: Se ha producido un error en el conteo de sesiones");
            }
            return usuariosConectados;
        }

        //consultar numero de id usando nombre de usuario
        public static string GetId(string name)
        {
            Logger.Info("[CheckScanServer] Se consume servicio GetId con nombre de usuario: {User}", name);
            var result = "";
            var query = $"SELECT * FROM {LocalSettings.schema}.SESIONES WHERE Nombre_maq=@name";

            try
            {
                if (LocalSettings.dbotipo.Equals("SQL", StringComparison.InvariantCultureIgnoreCase))
                {
                    //SQL SERVER
                    Logger.Info("[CheckScanServer] Conexion a base de datos SQL SERVER");
                    using (var connection = new SqlConnection(LocalSettings.connectionString))
                    {
                        connection.Open();
                        var row = connection.Query<Sesiones>(query, new { name }).FirstOrDefault();
                        if (row != null)
                        {
                            result = row.Id_sesion.ToString();
                        }
                        connection.Close();
                    }
                }
                else
                {
                    //ORACLE
                    query = LocalSettings.GetOracleQueryFormat(query, "@");
                    using (var connection = LocalSettings.GetOracleConnectionType())
                    {
                        connection.Open();
                        var row = connection.Query<Sesiones>(query, new { name }).FirstOrDefault();
                        if (row != null)
                        {
                            result = row.Id_sesion.ToString();
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[CheckScanServer] GetId: Error al buscar usuario: {User}", name);
            }
            return result;
        }

        //consultar username usando numero de id
        public static string GetUsername(string id)
        {
            Logger.Info("[CheckScanServer] Se consume servicio GetUsername con id: {UserId}", id);
            var result = "";
            var query = $"SELECT * FROM {LocalSettings.schema}.SESIONES WHERE ID_SESION=@id";

            try
            {
                if (LocalSettings.dbotipo.Equals("SQL", StringComparison.InvariantCultureIgnoreCase))
                {
                    //SQL SERVER
                    Logger.Info("[CheckScanServer] Conexion a base de datos SQL SERVER");
                    using (var connection = new SqlConnection(LocalSettings.connectionString))
                    {
                        connection.Open();
                        var row = connection.Query<Sesiones>(query, new { id }).FirstOrDefault();
                        if (row != null)
                        {
                            result = row.Username;
                        }
                        connection.Close();
                    }
                }
                else
                {
                    //ORACLE
                    query = LocalSettings.GetOracleQueryFormat(query, "@");
                    using (var connection = LocalSettings.GetOracleConnectionType())
                    {
                        connection.Open();
                        var row = connection.Query<Sesiones>(query, new { id }).FirstOrDefault();
                        if (row != null)
                        {
                            result = row.Username;
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[CheckScanServer] GetUsername: Error al buscar usuario:{IdSesion}", id);
            }
            return result;
        }

        /// <summary>
        /// Consultar token en base de datos parametrizando host e ip
        /// </summary>
        /// <param name="host"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string GetToken(string host, string ip)
        {
            Logger.Info("[CheckScanServer] Se consume servicio GetToken con host: {Host} ip: {Ip}", host, ip);
            var result = "";
            var query = $"SELECT * FROM {LocalSettings.schema}.SESIONES WHERE NOMBRE_MAQ = @host AND IP_MAQ = @ip";

            try
            {
                if (LocalSettings.dbotipo.Equals("SQL", StringComparison.InvariantCultureIgnoreCase))
                {
                    //SQL SERVER
                    Logger.Info("[CheckScanServer] Conexion a base de datos SQL SERVER");
                    using (var connection = new SqlConnection(LocalSettings.connectionString))
                    {
                        connection.Open();
                        var row = connection.Query<Sesiones>(query, new { host, ip }).FirstOrDefault();
                        if (row != null)
                        {
                            result = row.Token;
                        }
                        connection.Close();
                    }
                }
                else
                {
                    //ORACLE
                    query = LocalSettings.GetOracleQueryFormat(query, "@");
                    using (var connection = LocalSettings.GetOracleConnectionType())
                    {
                        connection.Open();
                        var row = connection.Query<Sesiones>(query, new { host, ip }).FirstOrDefault();
                        if (row != null)
                        {
                            result = row.Token;
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[CheckScanServer] GetToken: Error al encontrar token por host: {Host} e ip: {Ip}", host, ip);
            }
            return result;
        }

        public static bool EncontrarUsuario(string host, string ip)
        {
            Logger.Trace("[CheckScanServer / Timer de Sesión] Se consume servicio EncontrarUsuario con host: {Host} ip: {Ip}", host, ip);
            RevisarConexion(host);
            bool encontrado = false;
            var query = $"SELECT * FROM {LocalSettings.schema}.SESIONES WHERE Nombre_maq=@host AND IP_MAQ=@ip";
            try
            {
                if (LocalSettings.dbotipo.Equals("SQL", StringComparison.InvariantCultureIgnoreCase))
                {
                    //SQL SERVER
                    Logger.Trace("[CheckScanServer / Timer de Sesión] EncontrarUsuario: Conexion a base de datos SQL SERVER");
                    using (var connection = new SqlConnection(LocalSettings.connectionString))
                    {
                        connection.Open();
                        var row = connection.Query<Sesiones>(query, new { host, ip }).FirstOrDefault();
                        if (row != null)
                        {
                            encontrado = true;
                        }
                        connection.Close();
                        Logger.Trace("[CheckScanServer / Timer de Sesión] EncontrarUsuario (SQL) = Host: {Host} Encontrado: {Found}", host, encontrado);
                    }
                }
                else
                {
                    //ORACLE
                    query = LocalSettings.GetOracleQueryFormat(query, "@");
                    using (var connection = LocalSettings.GetOracleConnectionType())
                    {
                        connection.Open();
                        var row = connection.Query<Sesiones>(query, new { host, ip }).FirstOrDefault();
                        if (row != null)
                        {
                            encontrado = true;
                        }
                        connection.Close();
                        Logger.Trace("[CheckScanServer / Timer de Sesión] EncontrarUsuario (ORACLE) = Host: {Host} Encontrado: {Found}", host, encontrado);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[CheckScanServer / Timer de Sesión] EncontrarUsuario: Error al buscar usuario: {Host}", host);
            }
            return encontrado;
        }

        public static bool RevisarConexion(string name)
        {
            Logger.Trace("[CheckScanServer / Timer de Sesión] Se consume servicio RevisarConexion con host: {Host}", name);

            try
            {
                var sesion = new List<Sesiones>();
                var query = $"SELECT * FROM {LocalSettings.schema}.SESIONES WHERE Nombre_maq=@name";
                if (LocalSettings.dbotipo.Equals("SQL", StringComparison.InvariantCultureIgnoreCase))
                {
                    //SQL SERVER
                    Logger.Trace("[CheckScanServer / Timer de Sesión] RevisarConexion: Conexion a base de datos SQL SERVER");
                    using (var connection = new SqlConnection(LocalSettings.connectionString))
                    {
                        connection.Open();
                        sesion = connection.Query<Sesiones>(query, new { name }).ToList();
                        connection.Close();
                    }
                }
                else
                {
                    //ORACLE
                    query = LocalSettings.GetOracleQueryFormat(query, "@");
                    using (var connection = LocalSettings.GetOracleConnectionType())
                    {
                        connection.Open();
                        sesion = connection.Query<Sesiones>(query, new { name }).ToList();
                        connection.Close();
                    }
                }

                foreach (var row in sesion)
                {
                    var horaConexion = row.Hora;
                    var compararTiempo = DateTime.Now - horaConexion;
                    Logger.Trace("[CheckScanServer / Timer de Sesión] Revisando conexiones de usuarios => Minutos conexion de sesion: " +
                        "{TotalMinutes} / minutos configurados en WorkView: {ConfigMinutes}", compararTiempo.TotalMinutes, OnbaseGetData.GetMinutosConexion());

                    if (compararTiempo.TotalMinutes > OnbaseGetData.GetMinutosConexion())
                    {
                        Logger.Trace("[CheckScanServer / Timer de Sesión] RevisarConexion: El tiempo de conexion de la sesion ha sobrepasado el limite de minutos configurados en WorkView");
                        BorrarUsuario(row.Id_sesion);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[CheckScanServer / Timer de Sesión] RevisarConexion: Se ha producido un error al revisar conexion de host: {Host}", name);
            }
            return false;
        }

        public static List<Sesiones> GetSessions()
        {
            Logger.Info("[CheckScanServer] Se consume servicio GetSessions");
            var query = $"SELECT * FROM {LocalSettings.schema}.SESIONES";
            var sessions = new List<Sesiones>();
            try
            {
                if (LocalSettings.dbotipo.Equals("SQL", StringComparison.InvariantCultureIgnoreCase))
                {
                    //SQL SERVER
                    Logger.Info("[CheckScanServer] Conexion a base de datos SQL SERVER");
                    using (var connection = new SqlConnection(LocalSettings.connectionString))
                    {
                        connection.Open();
                        sessions = connection.Query<Sesiones>(query).ToList();
                        connection.Close();
                    }
                }
                else
                {
                    //ORACLE
                    using (var connection = LocalSettings.GetOracleConnectionType())
                    {
                        connection.Open();
                        sessions = connection.Query<Sesiones>(query).ToList();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[CheckScanServer] GetSessions: Error al listar usuarios");
            }
            return sessions;
        }
    }

    [Table(name: "Sesiones")]
    public class Sesiones
    {
        [Key]
        public int Id_sesion { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public string Nombre_maq { get; set; }
        public string Ip_maq { get; set; }
        public DateTime Hora { get; set; }
    }
}
