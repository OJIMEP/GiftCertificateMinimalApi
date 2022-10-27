namespace GiftCertificateMinimalApi.Data
{
    public static class Queries
    {
        public static string CertInfo { get; } =  @"SELECT
			_IDRRef AS Сертификат,
			_Fld4242 AS Штрихкод 
		INTO #Temp_CertRef
		FROM
			dbo._Reference172 --Справочник ПодарочныеСертификаты
		WHERE
			_Fld4242 IN(@Barcode)
		;
		SELECT
			T1._Fld14496RRef AS Сертификат,
			T1._Fld14497RRef AS Статус,
			T1._Fld26990 AS СрокДействия
		INTO #Temp_CertStatus
		FROM
			dbo._InfoRg14495 T1 --РС ИсторияПодарочныхСертификатов
			INNER JOIN (
				SELECT
					Max(_Period) AS Период,
					_Fld14496RRef
				FROM
					dbo._InfoRg14495
					INNER JOIN #Temp_CertRef 
					ON _Fld14496RRef = Сертификат 
				WHERE
					_Active = 0x01
					AND _Period <= Dateadd(Year, 2000, GETDATE())
				GROUP BY
					_Fld14496RRef) T2 
					ON T1._Fld14496RRef = T2._Fld14496RRef
						AND T1._Period = T2.Период
		;
		SELECT
			Сертификат,
			Sum(_Fld16861) AS Остаток
		INTO #Temp_SumLeft
		FROM
			#Temp_CertRef
			INNER JOIN dbo._AccumRgT16863 SumRemains --РН ПодарочныеСертификаты
				ON SumRemains._Fld16860RRef = #Temp_CertRef.Сертификат
		WHERE
			_Period = '5999-11-01T00:00:00'
		GROUP BY
			Сертификат
		HAVING Sum(_Fld16861) > 0
		;
		SELECT
			#Temp_CertRef.Штрихкод AS Barcode,
			#Temp_CertStatus.Статус AS CertStatus,
			CASE 
				WHEN #Temp_CertStatus.Статус IN (0xB3A1D155BEA215A74F77177CEA264869, 0x8EABEBCCF5A9FBF74BCB7DA9464028AE) -- Активирован, ЧастичноПогашен
					THEN 1
				ELSE 0
			END AS IsActive,
			CASE 
				WHEN ISNULL(#Temp_CertStatus.СрокДействия, @EmptyDate) > @DateNow
					THEN 1
				ELSE 0
			END AS IsValid,
			ISNULL(#Temp_SumLeft.Остаток, 0) AS SumLeft
		FROM #Temp_CertRef
			LEFT JOIN #Temp_CertStatus
			ON #Temp_CertRef.Сертификат = #Temp_CertStatus.Сертификат
			LEFT JOIN #Temp_SumLeft
			ON #Temp_CertRef.Сертификат = #Temp_SumLeft.Сертификат";

		public const string DatabaseBalancingReplicaFull = @"select datediff(ms, last_commit_time, getdate())
			from [master].[sys].[dm_hadr_database_replica_states]";

		public const string DatabaseBalancingMain = @"select top (1) _IDRRef from dbo._Reference112";

		public const string DatabaseBalancingReplicaTables = @"Select TOP(1) _IDRRef FROM dbo._Reference99";
	}
}
