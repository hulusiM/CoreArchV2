using CoreArchV2.Services.Interfaces;

namespace CoreArchV2.Services.Services
{
    public static class MvcHelper
    {
        public static string Pager<T>(IPagedList<T> model, string url = null)
        {
            if (model == null || model.TotalPages == 1 || model.TotalPages == 0)
                return "";

            var viewPageCount = 2;

            var start = model.CurrentPage - viewPageCount;
            var finish = model.CurrentPage + viewPageCount;

            if (start <= 3)
                start = 2;

            if (finish >= model.TotalPages - 2)
                finish = model.TotalPages - 1;

            if (model.CurrentPage == 1)
            {
                if (model.TotalPages > finish + 2)
                    finish = finish + 2;
                else if (model.TotalPages > finish + 1)
                    finish = finish + 1;
            }

            if (model.CurrentPage == 2)
                if (model.TotalPages > finish + 1)
                    finish = finish + 1;

            if (model.CurrentPage == model.TotalPages)
            {
                if (start > 3)
                    start = start - 2;
                else if (start > 2)
                    start = start - 1;
            }

            if (model.CurrentPage == model.TotalPages - 1)
                if (start > 2)
                    start = start - 1;

            if (start <= 3)
                start = 2;

            if (finish >= model.TotalPages - 2)
                finish = model.TotalPages - 1;

            var IsStartThreeDot = start > 2;
            var IsFinishThreeDot = finish < model.TotalPages - 1;

            var str = "";

            str +=
                "<div class='text-center' id='divPagedList' style='margin-top:30px' ><div id='totalPagedList' class='pull-left' style='line-height:47px; font-size: 12.5px;padding-left: 44px;'>" +
                model.TotalCount + " Kayıttan " + ((model.CurrentPage - 1) * model.PageSize + 1) + " - " +
                model.CurrentPage * model.PageSize +
                " Arası Kayıtlar</div><ul class='pagination pagination-flat  pull-right' >";

            if (model.HasPreviousPage)
                str += string.Format(
                    "<li><a href =\"javascript:funcFilterTable('{0}?page={1}')\">&larr; Önceki Sayfa</a></li>", url,
                    model.CurrentPage - 1);
            else
                str += "<li class='disabled'><a href='javascript:void(0);'>&larr; Önceki Sayfa</a></li>";

            str += string.Format(
                "<li class='{0}'><a class='paginate_button' href=\"javascript:funcFilterTable('{1}?page=1')\">1</a></li>",
                model.CurrentPage == 1 ? "active" : "", url);

            if (IsStartThreeDot)
                str += "<li class='disabled'><a style='cursor:default;' href='javascript:void(0);'>...</a></li>";

            for (var i = start; i <= finish; i++)
            {
                var cls = model.CurrentPage == i ? "active" : "";
                str += string.Format(
                    "<li class='{0}'><a class='paginate_button' href=\"javascript:funcFilterTable('{1}?page={2}')\">{2}</a></li>",
                    cls, url, i);
            }

            if (IsFinishThreeDot)
                str += "<li class='disabled'><a style='cursor:default;' href='javascript:void(0);'>...</a></li>";

            str += string.Format("<li class='{0}'><a href=\"javascript:funcFilterTable('{1}?page={2}')\">{2}</a></li>",
                model.CurrentPage == model.TotalPages ? "active" : "", url, model.TotalPages);

            if (model.HasNextPage)
                str += string.Format(
                    "<li><a href=\"javascript:funcFilterTable('{0}?page={1}')\">Sonraki Sayfa &rarr;</a></li>", url,
                    model.CurrentPage + 1);
            else
                str += "<li class='disabled'><a href='javascript:void(0);'>Sonraki Sayfa &rarr;</a></li>";

            str += "</ul></div>";
            return str;
        }
    }
}