using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using MovieBase.database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace MovieBase
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadUserData();

            if (AppSession.CurrentUser?.Roleid == 2)
            {
                AdminPanelBtn.Visibility = Visibility.Visible;
            }
            else
            {
                AdminPanelBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadUserData()
        {
            var user = AppSession.CurrentUser;
            if (user == null) return;

            var context = MovieBaseContext.GetContext();

            UsernameTxt.Text = user.Username;
            EmailTxt.Text = user.Email ?? "Почта не привязана";

            var userRole = context.Roles.FirstOrDefault(r => r.Roleid == user.Roleid);
            RoleTxt.Text = userRole?.Name?.ToUpper() ?? "ПОЛЬЗОВАТЕЛЬ";

            int reviewsCount = context.Reviews.Count(r => r.Userid == user.Userid);
            ReviewsCountTxt.Text = reviewsCount.ToString();

            int favoritesCount = context.Favorites.Count(f => f.Userid == user.Userid);
            FavoritesCountTxt.Text = favoritesCount.ToString();

            var userReviews = context.Reviews.Where(r => r.Userid == user.Userid).ToList();
            if (userReviews.Any())
            {
                double avg = userReviews.Average(r => (double)(r.Rating ?? 0));
                AvgRatingTxt.Text = avg.ToString("F1").Replace(',', '.');
            }
            else
            {
                AvgRatingTxt.Text = "—";
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AppSession.CurrentUser = null;

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                Window.GetWindow(this).Close();
            }
        }

        private void AdminPanelBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminPage());
        }

        private void ExportFavorites_Click(object sender, RoutedEventArgs e)
        {
            var user = AppSession.CurrentUser;
            if (user == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                FileName = $"Избранное_{user.Username}_{DateTime.Now:yyyyMMdd}"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            try
            {
                var context = MovieBaseContext.GetContext();
                var favorites = context.Favorites
                    .Where(f => f.Userid == user.Userid)
                    .Include(f => f.Movie)
                    .ToList();

                if (!favorites.Any())
                {
                    MessageBox.Show("Ваш список избранного пуст.");
                    return;
                }

                using (WordprocessingDocument wordDoc =
                    WordprocessingDocument.Create(saveFileDialog.FileName, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new W.Document();
                    W.Body body = mainPart.Document.AppendChild(new W.Body());

                    body.AppendChild(new W.SectionProperties(
                        new W.PageSize { Width = 11906, Height = 16838 },
                        new W.PageMargin
                        {
                            Top = 1134,
                            Bottom = 1134,
                            Left = 1134,
                            Right = 1134
                        }
                    ));

                    body.AppendChild(BuildBanner("★ Список избранного"));

                    body.AppendChild(BuildSubtitle(
                        $"Пользователь: {user.Username}   •   Дата: {DateTime.Now:dd MMMM yyyy}"));

                    body.AppendChild(new W.Paragraph());

                    W.Table table = BuildTable(favorites, user, context);
                    body.AppendChild(table);

                    body.AppendChild(new W.Paragraph());
                    body.AppendChild(BuildFootnote($"Всего фильмов в избранном: {favorites.Count}"));

                    mainPart.Document.Save();
                }

                MessageBox.Show("Файл успешно экспортирован!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}");
            }
        }

        private W.Table BuildTable(
            List<Favorite> favorites,
            User user,
            MovieBaseContext context)
        {
            int[] colWidths = { 5200, 1800, 2638 };

            var table = new W.Table();

            table.AppendChild(new W.TableProperties(
                new W.TableWidth { Width = "9638", Type = W.TableWidthUnitValues.Dxa },
                new W.TableLayout { Type = W.TableLayoutValues.Fixed },
                new W.TableBorders(
                    MakeBorder<W.TopBorder>(W.BorderValues.Single, "D32F2F", 4),
                    MakeBorder<W.BottomBorder>(W.BorderValues.Single, "D32F2F", 4),
                    MakeBorder<W.LeftBorder>(W.BorderValues.Single, "D32F2F", 4),
                    MakeBorder<W.RightBorder>(W.BorderValues.Single, "D32F2F", 4),
                    MakeBorder<W.InsideHorizontalBorder>(W.BorderValues.Single, "2A2A2A", 4),
                    MakeBorder<W.InsideVerticalBorder>(W.BorderValues.Single, "2A2A2A", 4)
                ),
                new W.TableCellMarginDefault(
                    new W.TopMargin { Width = "100", Type = W.TableWidthUnitValues.Dxa },
                    new W.BottomMargin { Width = "100", Type = W.TableWidthUnitValues.Dxa },
                    new W.StartMargin { Width = "140", Type = W.TableWidthUnitValues.Dxa },
                    new W.EndMargin { Width = "140", Type = W.TableWidthUnitValues.Dxa }
                )
            ));

            string[] headers = { "Название фильма", "Оценка", "Дата добавления" };
            var headerRow = new W.TableRow();
            for (int i = 0; i < headers.Length; i++)
                headerRow.AppendChild(BuildCell(headers[i], colWidths[i], isHeader: true));
            table.AppendChild(headerRow);

            bool alternate = false;
            foreach (var fav in favorites)
            {
                var review = context.Reviews
                    .FirstOrDefault(r => r.Userid == user.Userid && r.Movieid == fav.Movieid);

                string ratingText = review?.Rating.HasValue == true
                    ? $"{review.Rating}/10"
                    : "—";

                string dateText = fav.Date?.ToString("dd.MM.yyyy") ?? "—";

                var dataRow = new W.TableRow();
                dataRow.AppendChild(BuildCell(fav.Movie?.Title ?? "—", colWidths[0], isHeader: false, alternate));
                dataRow.AppendChild(BuildCell(ratingText, colWidths[1], isHeader: false, alternate, center: true));
                dataRow.AppendChild(BuildCell(dateText, colWidths[2], isHeader: false, alternate, center: true));

                table.AppendChild(dataRow);
                alternate = !alternate;
            }

            return table;
        }

        private W.TableCell BuildCell(
            string text, int widthTwip,
            bool isHeader,
            bool shaded = false,
            bool center = false)
        {
            var cell = new W.TableCell();

            cell.AppendChild(new W.TableCellProperties(
                new W.TableCellWidth { Width = widthTwip.ToString(), Type = W.TableWidthUnitValues.Dxa },
                new W.Shading
                {
                    Val = W.ShadingPatternValues.Clear,
                    Color = "auto",
                    Fill = isHeader ? "D32F2F" : (shaded ? "2A2A2A" : "1E1E1E")
                },
                new W.VerticalTextAlignmentOnPage { Val = W.VerticalJustificationValues.Center }
            ));

            var runProps = new W.RunProperties();
            runProps.AppendChild(new W.FontSize { Val = isHeader ? "20" : "18" });
            runProps.AppendChild(new W.Color { Val = isHeader ? "FFFFFF" : "E0E0E0" });
            if (isHeader) runProps.AppendChild(new W.Bold());

            var run = new W.Run();
            run.AppendChild(runProps);
            run.AppendChild(new W.Text(text) { Space = SpaceProcessingModeValues.Preserve });

            var paraProps = new W.ParagraphProperties();
            if (center || isHeader)
                paraProps.AppendChild(new W.Justification { Val = W.JustificationValues.Center });

            var para = new W.Paragraph();
            para.AppendChild(paraProps);
            para.AppendChild(run);

            cell.AppendChild(para);
            return cell;
        }

        private W.Paragraph BuildBanner(string text)
        {
            var pPr = new W.ParagraphProperties(
                new W.Justification { Val = W.JustificationValues.Center }
            );

            var rPr = new W.RunProperties(
                new W.Bold(),
                new W.Color { Val = "000000" },
                new W.FontSize { Val = "40" },
                new W.RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" }
            );

            var run = new W.Run();
            run.AppendChild(rPr);
            run.AppendChild(new W.Text(text));

            var para = new W.Paragraph();
            para.AppendChild(pPr);
            para.AppendChild(run);
            return para;
        }

        private W.Paragraph BuildSubtitle(string text)
        {
            var pPr = new W.ParagraphProperties(
                new W.Justification { Val = W.JustificationValues.Center },
                new W.SpacingBetweenLines { Before = "80", After = "80" }
            );

            var rPr = new W.RunProperties(
                new W.Color { Val = "000000" },
                new W.FontSize { Val = "20" },
                new W.Italic(),
                new W.RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" }
            );

            var run = new W.Run();
            run.AppendChild(rPr);
            run.AppendChild(new W.Text(text));

            var para = new W.Paragraph();
            para.AppendChild(pPr);
            para.AppendChild(run);
            return para;
        }

        private W.Paragraph BuildFootnote(string text)
        {
            var pPr = new W.ParagraphProperties(
                new W.Justification { Val = W.JustificationValues.Right },
                new W.SpacingBetweenLines { Before = "60" }
            );

            var rPr = new W.RunProperties(
                new W.Color { Val = "000000" },
                new W.FontSize { Val = "16" },
                new W.Italic(),
                new W.RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" }
            );

            var run = new W.Run();
            run.AppendChild(rPr);
            run.AppendChild(new W.Text(text));

            var para = new W.Paragraph();
            para.AppendChild(pPr);
            para.AppendChild(run);
            return para;
        }

        private T MakeBorder<T>(W.BorderValues style, string hexColor, uint size)
            where T : W.BorderType, new()
        {
            return new T
            {
                Val = new EnumValue<W.BorderValues>(style),
                Color = hexColor,
                Size = size,
                Space = 0
            };
        }

    }
}
