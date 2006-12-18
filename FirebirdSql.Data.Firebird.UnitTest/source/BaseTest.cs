/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using NUnit.Framework;
using System;
using System.Text;
using System.Collections;
using System.Configuration;
using System.Security.Cryptography;

namespace FirebirdSql.Data.Firebird.Tests
{
	public class BaseTest
	{
		#region Fields

		private FbConnection	connection;
		private FbTransaction	transaction;
		private bool			withTransaction;

		#endregion

		#region PROPERTIES

		public FbConnection Connection
		{
			get { return connection; }
		}

		public FbTransaction Transaction
		{
			get { return transaction; }
			set { transaction = value; }
		}

		#endregion

		#region CONSTRUCTORS
				
		public BaseTest()
		{
			this.withTransaction = false;
		}

		public BaseTest(bool withTransaction)
		{
			this.withTransaction = withTransaction;
		}

		#endregion

		#region NUNIT_METHODS

		[SetUp]
		public virtual void SetUp()
		{
			// Build the connection string
			StringBuilder connectionString = new StringBuilder();
			connectionString.AppendFormat(
				"User={0};Password={1};Database={2};DataSource={3};Port={4};Charset={5};Pooling={6}",
				ConfigurationSettings.AppSettings["User"],
				ConfigurationSettings.AppSettings["Password"],
				ConfigurationSettings.AppSettings["Database"],
				ConfigurationSettings.AppSettings["DataSource"],
				ConfigurationSettings.AppSettings["Port"],
				ConfigurationSettings.AppSettings["Charset"],
				ConfigurationSettings.AppSettings["Pooling"]);

			createDatabase(connectionString.ToString());
			createTables(connectionString.ToString());
			insertTestData(connectionString.ToString());
			createProcedures(connectionString.ToString());
			createTriggers(connectionString.ToString());
						
			connection = new FbConnection(connectionString.ToString());
			connection.Open();

			if (this.withTransaction)
			{
				transaction = connection.BeginTransaction();
			}
		}

		[TearDown]
		public virtual void TearDown()
		{			
			if (this.withTransaction)
			{
				try
				{
					transaction.Commit();
				}
				catch
				{
				}
			}
			connection.Close();
		}

		#endregion

		#region DATABASE_CREATION_METHODS

		private static void createDatabase(string connectionString)
		{
			Hashtable values = new Hashtable();

			values.Add("DataSource"	, 
				ConfigurationSettings.AppSettings["DataSource"]);
			values.Add("Database"	, 
				ConfigurationSettings.AppSettings["Database"]);
			values.Add("Port"		, 
				Convert.ToInt32(ConfigurationSettings.AppSettings["Port"]));
			values.Add("User"		, 
				ConfigurationSettings.AppSettings["User"]);
			values.Add("Password"	, 
				ConfigurationSettings.AppSettings["Password"]);
			values.Add("Dialect"	, 
				Convert.ToByte(ConfigurationSettings.AppSettings["Dialect"]));
			values.Add("ForcedWrite", 
				Convert.ToBoolean(ConfigurationSettings.AppSettings["ForcedWrite"]));
			values.Add("PageSize"	, 
				Convert.ToInt32(ConfigurationSettings.AppSettings["PageSize"]));
			values.Add("Charset"	, 
				ConfigurationSettings.AppSettings["Charset"]);
			values.Add("Overwrite", true);

			FbConnection.CreateDatabase(values);
		}

		private static void createTables(string connectionString)
		{
			FbConnection connection = new FbConnection(connectionString);
			connection.Open();

			StringBuilder commandText = new StringBuilder();

			// Table for general purpouse tests
			commandText.Append("CREATE TABLE TEST (");
			commandText.Append("INT_FIELD        INTEGER DEFAULT 0 NOT NULL PRIMARY KEY,");
			commandText.Append("CHAR_FIELD       CHAR(30),");
			commandText.Append("VARCHAR_FIELD    VARCHAR(100),");
			commandText.Append("BIGINT_FIELD     BIGINT,");
			commandText.Append("SMALLINT_FIELD   SMALLINT,");
			commandText.Append("DOUBLE_FIELD     DOUBLE PRECISION,");
			commandText.Append("FLOAT_FIELD		 FLOAT,");
			commandText.Append("NUMERIC_FIELD    NUMERIC(15,2),");
			commandText.Append("DECIMAL_FIELD    DECIMAL(15,2),");
			commandText.Append("DATE_FIELD       DATE,");
			commandText.Append("TIME_FIELD       TIME,");
			commandText.Append("TIMESTAMP_FIELD  TIMESTAMP,");
			commandText.Append("CLOB_FIELD       BLOB SUB_TYPE 1 SEGMENT SIZE 80,");
			commandText.Append("BLOB_FIELD       BLOB SUB_TYPE 0 SEGMENT SIZE 80,");
			commandText.Append("IARRAY_FIELD     INTEGER [1:4],");
			commandText.Append("SARRAY_FIELD     SMALLINT [1:5],");
			commandText.Append("LARRAY_FIELD     BIGINT [1:6],");
			commandText.Append("FARRAY_FIELD     FLOAT [1:4],");
			commandText.Append("BARRAY_FIELD     DOUBLE PRECISION [1:4],");
			commandText.Append("NARRAY_FIELD     NUMERIC(10,6) [1:4],");
			commandText.Append("DARRAY_FIELD     DATE [1:4],");
			commandText.Append("TARRAY_FIELD     TIME [1:4],");
			commandText.Append("TSARRAY_FIELD    TIMESTAMP [1:4],");
			commandText.Append("CARRAY_FIELD     CHAR(21) [1:4],");
			commandText.Append("VARRAY_FIELD     VARCHAR(30) [1:4],");
			commandText.Append("BIG_ARRAY        INTEGER [1:100000],");
			commandText.Append("EXPR_FIELD       COMPUTED BY (smallint_field * 1000),");
			commandText.Append("CS_FIELD         CHAR(1) CHARACTER SET UNICODE_FSS,");
			commandText.Append("UCCHAR_ARRAY     CHAR(10) [1:10] CHARACTER SET UNICODE_FSS);");
   
			FbCommand command = new FbCommand(commandText.ToString(), connection);
			command.ExecuteNonQuery();
			command.Dispose();
						
			connection.Close();
		}

		private void createProcedures(string connectionString)
		{
			FbConnection connection = new FbConnection(connectionString);
			connection.Open();

			StringBuilder commandText = new StringBuilder();

			// SELECT_DATA
			commandText = new StringBuilder();

			commandText.Append("CREATE PROCEDURE SELECT_DATA  \r\n");
			commandText.Append("RETURNS ( \r\n");
			commandText.Append("INT_FIELD INTEGER, \r\n");
			commandText.Append("VARCHAR_FIELD VARCHAR(100), \r\n");
			commandText.Append("DECIMAL_FIELD DECIMAL(15,2)) \r\n");
			commandText.Append("AS \r\n");
			commandText.Append("begin \r\n");
			commandText.Append("FOR SELECT INT_FIELD, VARCHAR_FIELD, DECIMAL_FIELD FROM TEST INTO :INT_FIELD, :VARCHAR_FIELD, :DECIMAL_FIELD \r\n");
			commandText.Append("DO \r\n");
			commandText.Append("SUSPEND; \r\n");
			commandText.Append("end;");

			FbCommand command = new FbCommand(commandText.ToString(), connection);
			command.ExecuteNonQuery();
			command.Dispose();

			// GETRECORDCOUNT
			commandText = new StringBuilder();

			commandText.Append("CREATE PROCEDURE GETRECORDCOUNT \r\n");
			commandText.Append("RETURNS ( \r\n");
			commandText.Append("RECCOUNT SMALLINT) \r\n");
			commandText.Append("AS \r\n");
			commandText.Append("begin \r\n");
			commandText.Append("for select count(*) from test into :reccount \r\n");
			commandText.Append("do \r\n");
			commandText.Append("suspend; \r\n");
			commandText.Append("end\r\n");

			command = new FbCommand(commandText.ToString(), connection);
			command.ExecuteNonQuery();
			command.Dispose();

			// GETVARCHARFIELD
			commandText = new StringBuilder();

			commandText.Append("CREATE PROCEDURE GETVARCHARFIELD (\r\n");
			commandText.Append("ID INTEGER)\r\n");
			commandText.Append("RETURNS (\r\n");
			commandText.Append("VARCHAR_FIELD VARCHAR(100))\r\n");
			commandText.Append("AS\r\n");
			commandText.Append("begin\r\n");
			commandText.Append("for select varchar_field from test where int_field = :id into :varchar_field\r\n");
			commandText.Append("do\r\n");
			commandText.Append("suspend;\r\n");
			commandText.Append("end\r\n");

			command = new FbCommand(commandText.ToString(), connection);
			command.ExecuteNonQuery();
			command.Dispose();

			// GETASCIIBLOB
			commandText = new StringBuilder();

			commandText.Append("CREATE PROCEDURE GETASCIIBLOB (\r\n");
			commandText.Append("ID INTEGER)\r\n");
			commandText.Append("RETURNS (\r\n");
			commandText.Append("ASCII_BLOB BLOB SUB_TYPE 1)\r\n");
			commandText.Append("AS\r\n");
			commandText.Append("begin\r\n");
			commandText.Append("for select clob_field from test where int_field = :id into :ascii_blob\r\n");
			commandText.Append("do\r\n");
			commandText.Append("suspend;\r\n");
			commandText.Append("end\r\n");

			command = new FbCommand(commandText.ToString(), connection);
			command.ExecuteNonQuery();
			command.Dispose();

			connection.Close();
		}

		private void createTriggers(string connectionString)
		{
			FbConnection connection = new FbConnection(connectionString);
			connection.Open();

			StringBuilder commandText = new StringBuilder();

			// new_row
			commandText = new StringBuilder();

			commandText.Append("CREATE TRIGGER new_row FOR test ACTIVE\r\n");
			commandText.Append("AFTER INSERT POSITION 0\r\n");
			commandText.Append("AS\r\n");
			commandText.Append("BEGIN\r\n");
			commandText.Append("POST_EVENT 'new row';\r\n");
			commandText.Append("END");

			FbCommand command = new FbCommand(commandText.ToString(), connection);
			command.ExecuteNonQuery();
			command.Dispose();

			// update_row

			commandText = new StringBuilder();

			commandText.Append("CREATE TRIGGER update_row FOR test ACTIVE\r\n");
			commandText.Append("AFTER UPDATE POSITION 0\r\n");
			commandText.Append("AS\r\n");
			commandText.Append("BEGIN\r\n");
			commandText.Append("POST_EVENT 'updated row';\r\n");
			commandText.Append("END");

			command = new FbCommand(commandText.ToString(), connection);
			command.ExecuteNonQuery();
			command.Dispose();

			connection.Close();
		}

		private void insertTestData(string connectionString)
		{
			FbConnection connection = new FbConnection(connectionString);
			connection.Open();

			StringBuilder commandText = new StringBuilder();

			commandText.Append("insert into test (int_field, char_field, varchar_field, bigint_field, smallint_field, float_field, double_field, numeric_field, date_field, time_field, timestamp_field, clob_field, blob_field)");
			commandText.Append(" values(@int_field, @char_field, @varchar_field, @bigint_field, @smallint_field, @float_field, @double_field, @numeric_field, @date_field, @time_field, @timestamp_field, @clob_field, @blob_field)");

			FbTransaction	transaction = connection.BeginTransaction();
			FbCommand		command		= new FbCommand(commandText.ToString(), connection, transaction);

			try
			{
				// Add command parameters
				command.Parameters.Add("@int_field"			, FbDbType.Integer);
				command.Parameters.Add("@char_field"			, FbDbType.Char);
				command.Parameters.Add("@varchar_field"		, FbDbType.VarChar);
				command.Parameters.Add("@bigint_field"		, FbDbType.BigInt);
				command.Parameters.Add("@smallint_field"		, FbDbType.SmallInt);
				command.Parameters.Add("@float_field"		, FbDbType.Double);
				command.Parameters.Add("@double_field"		, FbDbType.Double);
				command.Parameters.Add("@numeric_field"		, FbDbType.Numeric);
				command.Parameters.Add("@date_field"			, FbDbType.Date);
				command.Parameters.Add("@time_Field"			, FbDbType.Time);
				command.Parameters.Add("@timestamp_field"	, FbDbType.TimeStamp);
				command.Parameters.Add("@clob_field"			, FbDbType.Text);
				command.Parameters.Add("@blob_field"			, FbDbType.Binary);

				for (int i = 0; i < 100; i++)
				{
					command.Parameters["@int_field"].Value		= i;
					command.Parameters["@char_field"].Value		= "IRow " + i.ToString();
					command.Parameters["@varchar_field"].Value	= "IRow Number" + i.ToString();
					command.Parameters["@bigint_field"].Value	= i;
					command.Parameters["@smallint_field"].Value	= i;
					command.Parameters["@float_field"].Value		= (float)(i + 10)/5;
					command.Parameters["@double_field"].Value	= Math.Log(i, 10);
					command.Parameters["@numeric_field"].Value	= (decimal)(i + 10)/5;
					command.Parameters["@date_field"].Value		= DateTime.Now;
					command.Parameters["@time_field"].Value		= DateTime.Now;
					command.Parameters["@timestamp_field"].Value = DateTime.Now;
					command.Parameters["@clob_field"].Value		= "IRow Number" + i.ToString();
					command.Parameters["@blob_field"].Value		= Encoding.Default.GetBytes("IRow Number" + i.ToString());

					command.ExecuteNonQuery();
				}

				// Commit transaction
				transaction.Commit();
			}
			catch (FbException ex)
			{
				transaction.Rollback();
				throw ex;
			}
			finally
			{
				command.Dispose();
				connection.Close();
			}
		}

		#endregion

		#region Methods

		public int GetId()
		{
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

			byte[] buffer = new byte[4];
			
			rng.GetBytes(buffer);

			return BitConverter.ToInt32(buffer, 0);
		}

		#endregion
	}
}
