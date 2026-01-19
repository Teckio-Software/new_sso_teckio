using API_SSO.DTO;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace API_SSO.Procesos
{
    public class BaseDeDatosProceso
    {
        private readonly IConfiguration _configuracion;

        public BaseDeDatosProceso(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task CrearBaseDeDatos(string nombre)
        {
            var coneccion = _configuracion["connectionStrings:masterConnection"];
            try
            {
                var ruta = _configuracion["utilidades:scriptCreacion"];
                if(string.IsNullOrEmpty(coneccion)
                    || string.IsNullOrEmpty(ruta))
                {
                    return;
                }
                string script = File.ReadAllText(ruta);
                script = script.Replace("{{DB_NAME}}", nombre);
                IEnumerable<string> commandTexts = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                using (SqlConnection conn = new SqlConnection(coneccion))
                {
                    conn.Open();
                    foreach (string sqlBatch in commandTexts)
                    {
                        if (!string.IsNullOrWhiteSpace(sqlBatch))
                        {
                            using (SqlCommand cmd = new SqlCommand(sqlBatch, conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch
            {
                
            }
        }

        public async Task<bool> VerifyInstallation(string dbName)
        {
            var coneccion = _configuracion["connectionStrings:masterConnection"];
            if (string.IsNullOrEmpty(coneccion))
            {
                return false;
            }
            using (SqlConnection conn = new SqlConnection(coneccion))
            {
                conn.Open();

                // 1. Verificar si la base de datos existe
                string checkDb = "SELECT database_id FROM sys.databases WHERE name = @name";
                using (SqlCommand cmd = new SqlCommand(checkDb, conn))
                {
                    cmd.Parameters.AddWithValue("@name", dbName);
                    var result = cmd.ExecuteScalar();
                    if (result == null) return false;
                }

                // 2. Verificar si una tabla clave existe (ej. Almacen)
                // Cambiamos el contexto a la nueva DB
                conn.ChangeDatabase(dbName);
                string checkTable = "SELECT object_id FROM sys.tables WHERE name = 'Almacen'";
                using (SqlCommand cmd = new SqlCommand(checkTable, conn))
                {
                    var result = cmd.ExecuteScalar();
                    return result != null;
                }
            }
        }

        public async Task<int> CrearProyecto(ClienteCreacionDTO informacion, string nombreBD)
        {
            var coneccion = _configuracion["connectionStrings:ssoConection"];
            if (string.IsNullOrEmpty(coneccion))
            {
                return 0;
            }
            coneccion = coneccion.Replace("API_SSO", nombreBD);

            string query1 = """
                           INSERT INTO [dbo].[Proyecto]
                      ([CodigoProyecto]
                      ,[Nombre]
                      ,[NoSerie]
                      ,[Moneda]
                      ,[PresupuestoSinIva]
                      ,[TipoCambio]
                      ,[PresupuestoSinIvaMonedaNacional]
                      ,[PorcentajeIva]
                      ,[PresupuestoConIvaMonedaNacional]
                      ,[Anticipo]
                      ,[CodigoPostal]
                      ,[Domicilio]
                      ,[FechaInicio]
                      ,[FechaFinal]
                      ,[TipoProgramaActividad]
                      ,[InicioSemana]
                      ,[EsSabado]
                      ,[EsDomingo]
                      ,[IdPadre]
                      ,[Nivel]
                      ,[PorcentajeAvance])
                VALUES('
                """+informacion.CodigoProyecto+"""
                ','
                """+informacion.NombreProyecto+"""
                ',
                """+1+"""
                ,'
                """+informacion.Divisa+"""
                ',
                """+0.0+"""
                ,
                """+0.0+"""
                ,
                """+0.0+"""
                ,
                """+informacion.IVA+"""
                ,
                """+0.0+"""
                ,
                """+informacion.Anticipo+"""
                ,
                """+informacion.Cp+"""
                ,'
                """+informacion.UbicacionProyecto+"""
                ','
                """+informacion.FechaInicio+"""
                ','
                """+informacion.FechaFin+"""
                ',
                """+1+"""
                ,
                """+1+"""
                ,
                """+ (informacion.EsSabado ? 1 : 0) +"""
                ,
                """+ (informacion.EsDomingo ? 1 : 0) +"""
                ,
                """+ 0 +"""
                ,
                """+ 1 +"""
                ,
                """+ 0.0 + """
                );
                SELECT SCOPE_IDENTITY();
                """;

            using (SqlConnection connection = new SqlConnection(coneccion))
            {
                using (SqlCommand comand = new SqlCommand(query1, connection))
                {
                    connection.Open();
                    object resultado = comand.ExecuteScalar();
                    if (resultado != null)
                    {
                        return Convert.ToInt32(resultado);
                    }

                    return 0;
                }
            }

        }

        public async Task<int> CrearFSI(int idProyecto, string nombreBD)
        {
            var coneccion = _configuracion["connectionStrings:ssoConection"];
            if (string.IsNullOrEmpty(coneccion))
            {
                return 0;
            }
            coneccion = coneccion.Replace("API_SSO", nombreBD);

            string query = """
                           INSERT INTO [dbo].[FactorSalarioIntegrado]
                      ([IdProyecto]
                      ,[FSI])
                VALUES(
                """+idProyecto+"""
                      ,
                """+1+ """      
                      );
                SELECT SCOPE_IDENTITY();
                """;

            using (SqlConnection connection = new SqlConnection(coneccion))
            {
                using (SqlCommand comand = new SqlCommand(query, connection))
                {
                    connection.Open();
                    object resultado = comand.ExecuteScalar();
                    if (resultado != null)
                    {
                        return Convert.ToInt32(resultado);
                    }

                    return 0;
                }
            }
        }

        public async Task<int> CrearFSR(int idProyecto, string nombreBD)
        {
            var coneccion = _configuracion["connectionStrings:ssoConection"];
            if (string.IsNullOrEmpty(coneccion))
            {
                return 0;
            }
            coneccion = coneccion.Replace("API_SSO", nombreBD);

            string query = """
                           INSERT INTO [dbo].[FactorSalarioReal]
                ([IdProyecto]
                ,[PorcentajeFSR])
                VALUES(
                """ + idProyecto + """
                      ,
                """ + 1 + """      
                      );
                SELECT SCOPE_IDENTITY();
                """;

            using (SqlConnection connection = new SqlConnection(coneccion))
            {
                using (SqlCommand comand = new SqlCommand(query, connection))
                {
                    connection.Open();
                    object resultado = comand.ExecuteScalar();
                    if (resultado != null)
                    {
                        return Convert.ToInt32(resultado);
                    }

                    return 0;
                }
            }
        }

    }
}
