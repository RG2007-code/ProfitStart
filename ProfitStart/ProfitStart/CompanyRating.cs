using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitStart
{
    public static class CompanyRating
    {
        public static double GetAverage(int companyId)
        {
            var vals = Connection.connect.CompanyRatings
                .AsNoTracking()
                .Where(r => r.CompanyId == companyId
                         && r.IsApproved == true)
                .Select(r => r.Value)
                .ToList();

            if (vals.Count == 0) return 0;
            return vals.Average();
        }

        public static int GetCount(int companyId)
        {
            return Connection.connect.CompanyRatings
                .AsNoTracking()
                .Count(r => r.CompanyId == companyId
                         && r.IsApproved == true);
        }

        public static int GetMyRating(int companyId, int userId)
        {
            var r = Connection.connect.CompanyRatings
                .AsNoTracking()
                .FirstOrDefault(x => x.CompanyId == companyId
                                  && x.UserId == userId);
            return r?.Value ?? 0;
        }

        public static double Set(int companyId, int userId, int value)
        {
            if (value < 1) value = 1;
            if (value > 5) value = 5;

            var r = Connection.connect.CompanyRatings
                .FirstOrDefault(x => x.CompanyId == companyId
                                  && x.UserId == userId);

            if (r == null)
            {
                r = new CompanyRatings
                {
                    CompanyId = companyId,
                    UserId = userId,
                    Value = value,
                    RatedAt = DateTime.Now,
                    IsApproved = true,
                    IsModerated = false
                };
                Connection.connect.CompanyRatings.Add(r);
            }
            else
            {
                r.Value = value;
                r.RatedAt = DateTime.Now;

                if (r.IsApproved == false)
                    r.IsApproved = true;
            }

            Connection.connect.SaveChanges();
            return UpdateCompanyCache(companyId);
        }

        public static void SetComment(int companyId, int userId, string comment)
        {
            var r = Connection.connect.CompanyRatings
                .FirstOrDefault(x => x.CompanyId == companyId
                                  && x.UserId == userId);
            if (r == null) return;

            var text = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

            r.Comment = text;
            r.CommentedAt = text == null ? (DateTime?)null : DateTime.Now;

            if (text == null)
            {
                r.IsModerated = false;
                r.RejectionReason = null;
            }
            else
            {
                r.IsModerated = true;
                r.RejectionReason = null;
            }

            r.IsApproved = true;

            Connection.connect.SaveChanges();
        }

        public static List<CompanyReview> GetPendingReviews()
        {
            var raw = Connection.connect.CompanyRatings
                .AsNoTracking()
                .Where(r => r.Comment != null && r.Comment != ""
                         && r.IsModerated == true)
                .OrderByDescending(r => r.CommentedAt)
                .ToList();

            return BuildReviewList(raw);
        }

        public static void Approve(int reviewId)
        {
            var r = Connection.connect.CompanyRatings
                .FirstOrDefault(x => x.Id == reviewId);
            if (r == null) return;

            r.IsModerated = false;
            r.RejectionReason = null;
            r.IsApproved = true;
            Connection.connect.SaveChanges();

            var st = Connection.connect.Students
                .FirstOrDefault(s => s.UserId == r.UserId);

            if (st != null)
            {
                Notify.ToStudent(st.Id,
                    "Отзыв одобрен",
                    "Ваш отзыв о компании опубликован",
                    "company", r.CompanyId);
            }
        }

        public static void Reject(int reviewId, string reason)
        {
            var r = Connection.connect.CompanyRatings
                .FirstOrDefault(x => x.Id == reviewId);
            if (r == null) return;

            r.IsModerated = false;
            r.RejectionReason = reason;
            r.IsApproved = true;
            r.Comment = null;
            r.CommentedAt = null;
            Connection.connect.SaveChanges();

            var st = Connection.connect.Students
                .FirstOrDefault(s => s.UserId == r.UserId);

            if (st != null)
            {
                Notify.ToStudent(st.Id,
                    "Отзыв отклонён",
                    "Ваш отзыв не прошёл модерацию. Причина: " + reason,
                    "company", r.CompanyId);
            }
        }

        public static bool Delete(int reviewId, int userId)
        {
            var r = Connection.connect.CompanyRatings
                .FirstOrDefault(x => x.Id == reviewId);
            if (r == null) return false;
            if (r.UserId != userId) return false;

            int companyId = r.CompanyId;
            Connection.connect.CompanyRatings.Remove(r);
            Connection.connect.SaveChanges();
            UpdateCompanyCache(companyId);
            return true;
        }

        public static bool DeleteByAdmin(int reviewId)
        {
            var r = Connection.connect.CompanyRatings
                .FirstOrDefault(x => x.Id == reviewId);
            if (r == null) return false;

            int companyId = r.CompanyId;
            Connection.connect.CompanyRatings.Remove(r);
            Connection.connect.SaveChanges();
            UpdateCompanyCache(companyId);
            return true;
        }

        public static void SetCompanyReply(int reviewId, string reply)
        {
            var r = Connection.connect.CompanyRatings
                .FirstOrDefault(x => x.Id == reviewId);
            if (r == null) return;

            r.CompanyReply = string.IsNullOrWhiteSpace(reply) ? null : reply.Trim();
            r.CompanyReplyAt = DateTime.Now;
            Connection.connect.SaveChanges();
        }

        public static void DeleteCompanyReply(int reviewId)
        {
            var r = Connection.connect.CompanyRatings
                .FirstOrDefault(x => x.Id == reviewId);
            if (r == null) return;

            r.CompanyReply = null;
            r.CompanyReplyAt = null;
            Connection.connect.SaveChanges();
        }

        public static List<CompanyReview> GetReviews(int companyId, int take = 100)
        {
            var raw = Connection.connect.CompanyRatings
                .AsNoTracking()
                .Where(r => r.CompanyId == companyId
                         && r.Comment != null
                         && r.Comment != ""
                         && r.IsModerated == false
                         && r.IsApproved == true)
                .OrderByDescending(r => r.CommentedAt)
                .Take(take)
                .ToList();

            return BuildReviewList(raw);
        }

        private static List<CompanyReview> BuildReviewList(List<CompanyRatings> raw)
        {
            var result = new List<CompanyReview>();

            foreach (var r in raw)
            {
                var u = Connection.connect.Users
                    .AsNoTracking()
                    .FirstOrDefault(x => x.Id == r.UserId);

                string name = "Пользователь";
                byte[] avatar = null;

                if (u != null)
                {
                    avatar = u.AvatarData;

                    var s = Connection.connect.Students
                        .AsNoTracking()
                        .FirstOrDefault(x => x.UserId == u.Id);

                    name = s?.FullName ?? u.Email;
                }

                result.Add(new CompanyReview
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = name,
                    UserAvatar = avatar,
                    Value = r.Value,
                    Comment = r.Comment,
                    CompanyReply = r.CompanyReply,
                    CompanyReplyAt = r.CompanyReplyAt,
                    CreatedAt = r.CommentedAt ?? r.RatedAt ?? DateTime.Now
                });
            }

            return result;
        }

        private static double UpdateCompanyCache(int companyId)
        {
            double avg = GetAverage(companyId);

            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == companyId);
            if (c != null)
            {
                c.Rating = (decimal)avg;
                Connection.connect.SaveChanges();
            }

            return avg;
        }
    }

    public class CompanyReview
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public byte[] UserAvatar { get; set; }
        public int Value { get; set; }
        public string Comment { get; set; }
        public string CompanyReply { get; set; }
        public DateTime? CompanyReplyAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Stars =>
            new string('★', Value) + new string('☆', 5 - Value);

        public string DateText =>
            CreatedAt.ToString("dd.MM.yyyy");

        public string CompanyReplyText =>
            string.IsNullOrEmpty(CompanyReply)
                ? ""
                : "Ответ компании: " + CompanyReply;

        public System.Windows.Visibility ReplyVisibility =>
            string.IsNullOrEmpty(CompanyReply)
                ? System.Windows.Visibility.Collapsed
                : System.Windows.Visibility.Visible;
    }
}