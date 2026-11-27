using System.Collections.Generic;
using UnityEngine;

namespace LumiLantern
{
    [System.Serializable]
    public class ChapterInfo
    {
        public int chapterNumber;
        public string title;
        public string subtitle;
        public string mechanicDescription;
        public int totalLevels;
        public int requiredStarsToUnlock;
    }

    public static class LevelDatabase
    {
        private static List<LevelData> levels;
        private static List<ChapterInfo> chapters;

        public static List<ChapterInfo> GetChapters()
        {
            if (chapters == null) InitChapters();
            return chapters;
        }

        public static List<LevelData> GetAllLevels()
        {
            if (levels == null) InitLevels();
            return levels;
        }

        public static LevelData GetLevel(int number)
        {
            if (levels == null) InitLevels();
            if (number < 1 || number > levels.Count) return levels[0];
            return levels[number - 1];
        }

        private static void InitChapters()
        {
            chapters = new List<ChapterInfo>
            {
                new ChapterInfo
                {
                    chapterNumber = 1,
                    title = "Chương 1 · Ánh sáng đầu tiên",
                    subtitle = "Gương và tường",
                    mechanicDescription = "Học luật phản xạ 90°, đi vòng vật cản",
                    totalLevels = 5,
                    requiredStarsToUnlock = 0
                },
                new ChapterInfo
                {
                    chapterNumber = 2,
                    title = "Chương 2 · Sắc màu",
                    subtitle = "Kính lọc đỏ, xanh",
                    mechanicDescription = "Đèn có màu chỉ sáng khi tia mang đúng màu",
                    totalLevels = 2,
                    requiredStarsToUnlock = 5
                },
                new ChapterInfo
                {
                    chapterNumber = 3,
                    title = "Chương 3 · Chia đôi",
                    subtitle = "Bộ chia tia",
                    mechanicDescription = "Một tia tách thành hai nhánh cân bằng",
                    totalLevels = 2,
                    requiredStarsToUnlock = 10
                },
                new ChapterInfo
                {
                    chapterNumber = 4,
                    title = "Chương 4 · Dấu vết cũ",
                    subtitle = "Gương cố định",
                    mechanicDescription = "Tận dụng hoặc né các gương cố định sẵn",
                    totalLevels = 3,
                    requiredStarsToUnlock = 15
                },
                new ChapterInfo
                {
                    chapterNumber = 5,
                    title = "Chương 5 · Cổng nhịp",
                    subtitle = "Cổng chẵn–lẻ",
                    mechanicDescription = "Cổng mở khi số gương lẻ, đóng khi chẵn",
                    totalLevels = 3,
                    requiredStarsToUnlock = 22
                }
            };
        }

        private static void InitLevels()
        {
            levels = new List<LevelData>();

            // --- CHƯƠNG 1 ---
            // Màn 1: Phản xạ đầu tiên
            levels.Add(new LevelData
            {
                levelNumber = 1,
                levelName = "Phản xạ đầu tiên",
                chapterNumber = 1,
                chapterName = "Chương 1 · Ánh sáng đầu tiên",
                rows = 5, cols = 5,
                maxMirrors = 1, minMirrors = 1,
                tutorialText = "Chạm vào ô viền vàng để đặt gương phản xạ tia sáng.",
                gridAscii =
                    "...L.\n" +
                    ".....\n" +
                    ">....\n" +
                    ".....\n" +
                    ".....",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(3, 4, MirrorType.Slash)
                }
            });

            // Màn 2: Tia xuyên đèn
            levels.Add(new LevelData
            {
                levelNumber = 2,
                levelName = "Tia xuyên đèn",
                chapterNumber = 1,
                chapterName = "Chương 1 · Ánh sáng đầu tiên",
                rows = 5, cols = 5,
                maxMirrors = 2, minMirrors = 2,
                tutorialText = "Tia sáng không dừng lại ở đèn. Một tia có thể thắp nhiều đèn.",
                gridAscii =
                    ".L...\n" +
                    "...L.\n" +
                    ">....\n" +
                    ".....\n" +
                    ".....",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(3, 4, MirrorType.Slash),
                    new MirrorPlacement(1, 4, MirrorType.Backslash)
                }
            });

            // Màn 3: Đi vòng vật cản
            levels.Add(new LevelData
            {
                levelNumber = 3,
                levelName = "Đi vòng vật cản",
                chapterNumber = 1,
                chapterName = "Chương 1 · Ánh sáng đầu tiên",
                rows = 5, cols = 5,
                maxMirrors = 3, minMirrors = 3,
                tutorialText = "Tường chặn tia sáng. Đặt gương để dẫn tia đi vòng qua tường.",
                gridAscii =
                    ".....\n" +
                    ".....\n" +
                    ">.#.L\n" +
                    ".....\n" +
                    ".....",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(3, 2, MirrorType.Slash),
                    new MirrorPlacement(2, 2, MirrorType.Slash),
                    new MirrorPlacement(2, 5, MirrorType.Backslash)
                }
            });

            // Màn 4: Hành lang hẹp
            levels.Add(new LevelData
            {
                levelNumber = 4,
                levelName = "Hành lang hẹp",
                chapterNumber = 1,
                chapterName = "Chương 1 · Ánh sáng đầu tiên",
                rows = 5, cols = 5,
                maxMirrors = 3, minMirrors = 3,
                tutorialText = "Quan sát kỹ hành lang hẹp giữa các bức tường.",
                gridAscii =
                    ".##L.\n" +
                    ">.#..\n" +
                    "..#..\n" +
                    ".....\n" +
                    "..#..",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(2, 2, MirrorType.Backslash),
                    new MirrorPlacement(4, 2, MirrorType.Backslash),
                    new MirrorPlacement(4, 4, MirrorType.Slash)
                }
            });

            // Màn 5: Ba đèn một tia
            levels.Add(new LevelData
            {
                levelNumber = 5,
                levelName = "Ba đèn một tia",
                chapterNumber = 1,
                chapterName = "Chương 1 · Ánh sáng đầu tiên",
                rows = 5, cols = 5,
                maxMirrors = 3, minMirrors = 3,
                tutorialText = "Lên kế hoạch đường đi để thắp sáng cả 3 chiếc đèn lồng.",
                gridAscii =
                    ".#.L.\n" +
                    "..#L.\n" +
                    "..L..\n" +
                    ">....\n" +
                    "#.##.",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(3, 2, MirrorType.Slash),
                    new MirrorPlacement(3, 4, MirrorType.Slash),
                    new MirrorPlacement(4, 2, MirrorType.Slash)
                }
            });

            // --- CHƯƠNG 2 ---
            // Màn 6: Cửa kính đỏ
            levels.Add(new LevelData
            {
                levelNumber = 6,
                levelName = "Cửa kính đỏ",
                chapterNumber = 2,
                chapterName = "Chương 2 · Sắc màu",
                rows = 5, cols = 5,
                maxMirrors = 2, minMirrors = 2,
                tutorialText = "Kính đỏ (hình tròn) đổi tia trắng thành tia đỏ để thắp đèn đỏ.",
                gridAscii =
                    "#....\n" +
                    ">....\n" +
                    "..rRR\n" +
                    "#....\n" +
                    "....#",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(2, 2, MirrorType.Backslash),
                    new MirrorPlacement(3, 2, MirrorType.Backslash)
                }
            });

            // Màn 7: Màu xanh lên tiếng
            levels.Add(new LevelData
            {
                levelNumber = 7,
                levelName = "Màu xanh lên tiếng",
                chapterNumber = 2,
                chapterName = "Chương 2 · Sắc màu",
                rows = 5, cols = 5,
                maxMirrors = 3, minMirrors = 3,
                tutorialText = "Kính xanh (hình thoi) biến tia sáng thành màu xanh lam.",
                gridAscii =
                    ".#.v#\n" +
                    ".....\n" +
                    "..b..\n" +
                    "##.BB\n" +
                    ".....",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(2, 3, MirrorType.Slash),
                    new MirrorPlacement(2, 4, MirrorType.Slash),
                    new MirrorPlacement(4, 3, MirrorType.Backslash)
                }
            });

            // --- CHƯƠNG 3 ---
            // Màn 8: Tia tách đôi
            levels.Add(new LevelData
            {
                levelNumber = 8,
                levelName = "Tia tách đôi",
                chapterNumber = 3,
                chapterName = "Chương 3 · Chia đôi",
                rows = 5, cols = 5,
                maxMirrors = 2, minMirrors = 2,
                tutorialText = "Bộ chia tia Q tách 1 tia thành 2 nhánh: đi thẳng và phản xạ.",
                gridAscii =
                    "..L..\n" +
                    "L.Q..\n" +
                    "#.L..\n" +
                    "...#.\n" +
                    ".^#..",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(4, 2, MirrorType.Slash),
                    new MirrorPlacement(4, 3, MirrorType.Slash)
                }
            });

            // Màn 9: Hai nhánh ba đèn
            levels.Add(new LevelData
            {
                levelNumber = 9,
                levelName = "Hai nhánh ba đèn",
                chapterNumber = 3,
                chapterName = "Chương 3 · Chia đôi",
                rows = 5, cols = 5,
                maxMirrors = 3, minMirrors = 3,
                tutorialText = "Dùng bộ chia tia để thắp các đèn ở cả 2 nhánh khác nhau.",
                gridAscii =
                    "L..#.\n" +
                    "L....\n" +
                    "Q.#..\n" +
                    "#.L.#\n" +
                    "...^.",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(3, 2, MirrorType.Backslash),
                    new MirrorPlacement(4, 2, MirrorType.Backslash),
                    new MirrorPlacement(4, 4, MirrorType.Backslash)
                }
            });

            // --- CHƯƠNG 4 ---
            // Màn 10: Gương cố định
            levels.Add(new LevelData
            {
                levelNumber = 10,
                levelName = "Gương cố định",
                chapterNumber = 4,
                chapterName = "Chương 4 · Dấu vết cũ",
                rows = 5, cols = 5,
                maxMirrors = 2, minMirrors = 2,
                tutorialText = "Gương cố định 'n' không thể gỡ hay xoay. Hãy tận dụng nó!",
                gridAscii =
                    "..L..\n" +
                    ">....\n" +
                    "#.L..\n" +
                    ".n...\n" +
                    ".##..",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(2, 2, MirrorType.Backslash),
                    new MirrorPlacement(4, 3, MirrorType.Slash)
                }
            });

            // Màn 11: Đỏ và gương cố định
            levels.Add(new LevelData
            {
                levelNumber = 11,
                levelName = "Đỏ và gương cố định",
                chapterNumber = 4,
                chapterName = "Chương 4 · Dấu vết cũ",
                rows = 5, cols = 5,
                maxMirrors = 3, minMirrors = 3,
                tutorialText = "Kết hợp kính đỏ với gương cố định để dẫn tia tới đích.",
                gridAscii =
                    ".....\n" +
                    ">L...\n" +
                    "#.r.R\n" +
                    "#.n.#\n" +
                    ".....",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(2, 3, MirrorType.Backslash),
                    new MirrorPlacement(3, 4, MirrorType.Slash),
                    new MirrorPlacement(4, 4, MirrorType.Slash)
                }
            });

            // Màn 12: Hai gương gần sẵn
            levels.Add(new LevelData
            {
                levelNumber = 12,
                levelName = "Hai gương gần sẵn",
                chapterNumber = 4,
                chapterName = "Chương 4 · Dấu vết cũ",
                rows = 5, cols = 5,
                maxMirrors = 3, minMirrors = 3,
                tutorialText = "Tia xuất phát từ bên phải (<). Chú ý hướng phản xạ của m và n.",
                gridAscii =
                    ".L#..\n" +
                    "#.n..\n" +
                    "..Lm<\n" +
                    ".....\n" +
                    "..#..",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(2, 2, MirrorType.Backslash),
                    new MirrorPlacement(4, 3, MirrorType.Backslash),
                    new MirrorPlacement(4, 4, MirrorType.Slash)
                }
            });

            // --- CHƯƠNG 5 ---
            // Màn 13: Cổng nhịp đầu tiên
            levels.Add(new LevelData
            {
                levelNumber = 13,
                levelName = "Cổng nhịp đầu tiên",
                chapterNumber = 5,
                chapterName = "Chương 5 · Cổng nhịp",
                rows = 5, cols = 5,
                maxMirrors = 3, minMirrors = 3,
                tutorialText = "Cổng nhịp G chỉ mở khi tổng số gương bạn đặt là số LẺ!",
                gridAscii =
                    ".....\n" +
                    "#....\n" +
                    "..L..\n" +
                    ".G#.<\n" +
                    ".L...",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(3, 2, MirrorType.Slash),
                    new MirrorPlacement(3, 4, MirrorType.Backslash),
                    new MirrorPlacement(4, 4, MirrorType.Backslash)
                }
            });

            // Màn 14: Nhịp và tách
            levels.Add(new LevelData
            {
                levelNumber = 14,
                levelName = "Nhịp và tách",
                chapterNumber = 5,
                chapterName = "Chương 5 · Cổng nhịp",
                rows = 5, cols = 5,
                maxMirrors = 3, minMirrors = 3,
                tutorialText = "Kết hợp cổng nhịp và bộ chia tia P để thắp sáng cả 3 đèn.",
                gridAscii =
                    ".PLL.\n" +
                    "..L..\n" +
                    "##.G.\n" +
                    "....<\n" +
                    "...#.",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(2, 2, MirrorType.Backslash),
                    new MirrorPlacement(2, 4, MirrorType.Backslash),
                    new MirrorPlacement(4, 4, MirrorType.Backslash)
                }
            });

            // Màn 15: Màn tổng hợp
            levels.Add(new LevelData
            {
                levelNumber = 15,
                levelName = "Màn tổng hợp",
                chapterNumber = 5,
                chapterName = "Chương 5 · Cổng nhịp",
                rows = 5, cols = 5,
                maxMirrors = 3, minMirrors = 3,
                tutorialText = "Thử thách đỉnh cao kết hợp kính đỏ, gương cố định và cổng nhịp!",
                gridAscii =
                    "..#..\n" +
                    ".....\n" +
                    ".L.n<\n" +
                    "..rRG\n" +
                    ".#.#.",
                solution = new List<MirrorPlacement>
                {
                    new MirrorPlacement(2, 2, MirrorType.Slash),
                    new MirrorPlacement(2, 4, MirrorType.Backslash),
                    new MirrorPlacement(4, 2, MirrorType.Backslash)
                }
            });

            // Parse all grids
            foreach (var lvl in levels)
            {
                lvl.ParseGrid();
            }
        }
    }
}
