using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KKSCharaDownloader
{
	public static class UploaderPramQuery
	{
		/// <summary>
		/// クエリ作成
		/// </summary>
		/// <param name="page">100件ずつ</param>
		/// <param name="sex">true;女 false:男</param>
		public static string CreateUrlQuery(int page, bool sex)
		{
			var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

			queryString["page"] = page.ToString();
			if (sex)
				queryString["sex"] = "1";
			else
				queryString["sex"] = "0";

			queryString["order_by"] = "dlcount,desc";

			queryString["limit"] = "100";

			return queryString.ToString();
		}


	}
}
