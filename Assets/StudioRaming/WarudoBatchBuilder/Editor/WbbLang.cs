using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public enum WbbLanguage
    {
        Korean = 0,
        English = 1,
        Japanese = 2,
    }

    public static partial class WbbLang
    {
        private const string LanguagePrefKey = WbbUtil.PrefPrefix + "Language";
        private static readonly Dictionary<string, string[]> Texts = new Dictionary<string, string[]>();
        static WbbLang()
        {
            Merge(CommonTexts);
            Merge(BuildTexts);
            Merge(AutoScriptTexts);
            Merge(AsmdefTexts);
            Merge(BuildListTexts);
            Merge(LibraryTexts);
            Merge(HistoryTexts);
            Merge(RunnerTexts);
            Merge(FolderTexts);
            Merge(PackageTexts);
            Merge(QuickTexts);
            Merge(SafetyTexts);
            Merge(UiTexts);
            Merge(FlowTexts);
            Merge(QuickIterateTexts);
        }

        private static void Merge(Dictionary<string, string[]> source)
        {
            if (source == null)
                return;
            foreach (var kv in source)
            {
                if (Texts.ContainsKey(kv.Key))
                    Debug.LogWarning("[WarudoBatchBuilder] Duplicate language key: " + kv.Key);
                Texts[kv.Key] = kv.Value;
            }
        }

        private static int version;
        private static int seenSharedVersion;
        public static int Version
        {
            get
            {
                SyncSharedVersion();
                return version;
            }
        }

        private static void SyncSharedVersion()
        {
            int shared = UI.SRLanguage.Version;
            if (shared == seenSharedVersion)
                return;
            seenSharedVersion = shared;
            version++;
        }

        public static WbbLanguage Current
        {
            get
            {
                return (WbbLanguage)(int)UI.SRLanguage.Get(LanguagePrefKey);
            }

            set
            {
                UI.SRLanguage.Set((UI.SRLang)(int)value, LanguagePrefKey);
                SyncSharedVersion();
            }
        }

        public static void OnWindowFocus()
        {
            UI.SRLanguage.Invalidate();
            SyncSharedVersion();
        }

        public static string T(string key)
        {
            string[] localized;
            if (Texts.TryGetValue(key, out localized) && localized != null && localized.Length > 0)
                return Localize(key, localized, (int)Current);
            Debug.LogWarning("[WarudoBatchBuilder] Missing language key: " + key);
            return key;
        }

        public static string T(string key, params object[] args)
        {
            return string.Format(T(key), args);
        }

        public static bool Has(string key)
        {
            return Texts.ContainsKey(key);
        }

        private class ContentEntry
        {
            public int ver;
            public GUIContent gc;
        }

        private static readonly Dictionary<string, ContentEntry> contentCache = new Dictionary<string, ContentEntry>();
        public static GUIContent C(string key)
        {
            int ver = Version;
            ContentEntry entry;
            if (contentCache.TryGetValue(key, out entry) && entry.ver == ver)
                return entry.gc;
            var gc = new GUIContent(T(key), Has(key + ".tip") ? T(key + ".tip") : string.Empty);
            contentCache[key] = new ContentEntry
            {
                ver = ver,
                gc = gc
            };
            return gc;
        }

        public static string TypeName(WbbExportType type)
        {
            switch (type)
            {
                case WbbExportType.Character:
                    return T("Common.TypeCharacter");
                case WbbExportType.Environment:
                    return T("Common.TypeEnvironment");
                default:
                    return T("Common.TypeProp");
            }
        }

        public static string StatusName(WbbBuildStatus status)
        {
            switch (status)
            {
                case WbbBuildStatus.Running:
                    return T("Common.StatusRunning");
                case WbbBuildStatus.Success:
                    return T("Common.StatusSuccess");
                case WbbBuildStatus.Failed:
                    return T("Common.StatusFailed");
                case WbbBuildStatus.Canceled:
                    return T("Common.StatusCanceled");
                case WbbBuildStatus.Skipped:
                    return T("Common.StatusSkipped");
                default:
                    return T("Common.StatusPending");
            }
        }

        public static readonly string[] LanguageLabels =
        {
            "한국어",
            "English",
            "日本語"
        };
        private static readonly Dictionary<string, string[]> CommonTexts = new Dictionary<string, string[]>
        {
            {
                "Common.ProductName",
                new[]
                {
                    "WARUDO Mod Mate",
                    "WARUDO Mod Mate",
                    "WARUDO Mod Mate"
                }
            },
            {
                "Tab.Build",
                new[]
                {
                    "빌드",
                    "Build",
                    "ビルド"
                }
            },
            {
                "Tab.Build.tip",
                new[]
                {
                    "프로젝트의 프리팹을 골라 여러 모드를 한 번에 빌드하거나 .unitypackage 로 내보냅니다.",
                    "Pick prefabs from the project and build many mods in one run, or export them as .unitypackage.",
                    "プロジェクトのプレハブを選んで複数のMODを一括ビルドするか、.unitypackage で書き出します。"
                }
            },
            {
                "Tab.Library",
                new[]
                {
                    "모드 목록",
                    "Library",
                    "MOD一覧"
                }
            },
            {
                "Tab.Library.tip",
                new[]
                {
                    "Warudo에 설치된 산출물(.warudo)을 검색·정리하고 빌드 이력과 대조합니다.",
                    "Search installed .warudo outputs and cross-check them against build history.",
                    "Warudoにインストール済みの.warudoを検索・整理し、ビルド履歴と照合します。"
                }
            },
            {
                "Tab.History",
                new[]
                {
                    "빌드 이력",
                    "History",
                    "ビルド履歴"
                }
            },
            {
                "Tab.History.tip",
                new[]
                {
                    "언제 무엇을 어떤 버전으로 빌드했는지 전부 기록됩니다. 실패 항목만 재빌드할 수 있습니다.",
                    "Every build is recorded with time, version and result. Failed items can be rebuilt.",
                    "いつ何をどのバージョンでビルドしたか全て記録されます。失敗分だけ再ビルドできます。"
                }
            },
            {
                "Common.Guide",
                new[]
                {
                    "사용법 안내",
                    "How to use",
                    "使い方ガイド"
                }
            },
            {
                "Common.Guide.tip",
                new[]
                {
                    "이 탭의 사용 순서를 펼쳐서 봅니다.",
                    "Expand the step-by-step guide for this tab.",
                    "このタブの手順を展開して表示します。"
                }
            },
            {
                "Common.Search",
                new[]
                {
                    "검색",
                    "Search",
                    "検索"
                }
            },
            {
                "Common.Search.tip",
                new[]
                {
                    "이름으로 걸러냅니다. 공백으로 여러 단어를 넣으면 모두 포함된 항목만 남습니다.",
                    "Filter by name. Multiple space-separated words must all match.",
                    "名前で絞り込みます。スペース区切りの語は全て含む項目のみ残ります。"
                }
            },
            {
                "Common.SearchClear",
                new[]
                {
                    "지우기",
                    "Clear",
                    "クリア"
                }
            },
            {
                "Common.Refresh",
                new[]
                {
                    "새로고침",
                    "Refresh",
                    "更新"
                }
            },
            {
                "Common.Refresh.tip",
                new[]
                {
                    "목록을 다시 읽습니다.",
                    "Re-read the list from disk.",
                    "リストを読み直します。"
                }
            },
            {
                "Common.SelectAll",
                new[]
                {
                    "전체 선택",
                    "Select All",
                    "全選択"
                }
            },
            {
                "Common.SelectAll.tip",
                new[]
                {
                    "지금 보이는 항목만 선택합니다.",
                    "Selects only the items visible right now.",
                    "今表示されている項目だけを選択します。"
                }
            },
            {
                "Common.DeselectAll.tip",
                new[]
                {
                    "보이지 않는 항목을 포함해 선택을 모두 해제합니다.",
                    "Clears every selection, including items hidden by a search or filter.",
                    "検索・フィルターで隠れている項目を含め、選択をすべて解除します。"
                }
            },
            {
                "Common.DeselectAll",
                new[]
                {
                    "전체 해제",
                    "Clear Selection",
                    "全解除"
                }
            },
            {
                "Common.Selected",
                new[]
                {
                    "선택 {0}개",
                    "{0} selected",
                    "選択 {0}件"
                }
            },
            {
                "Common.Ok",
                new[]
                {
                    "확인",
                    "OK",
                    "OK"
                }
            },
            {
                "Common.Cancel",
                new[]
                {
                    "취소",
                    "Cancel",
                    "キャンセル"
                }
            },
            {
                "Common.Close",
                new[]
                {
                    "닫기",
                    "Close",
                    "閉じる"
                }
            },
            {
                "Common.Delete",
                new[]
                {
                    "삭제",
                    "Delete",
                    "削除"
                }
            },
            {
                "Common.None",
                new[]
                {
                    "(없음)",
                    "(none)",
                    "(なし)"
                }
            },
            {
                "Common.OpenFolder",
                new[]
                {
                    "폴더 열기",
                    "Open Folder",
                    "フォルダを開く"
                }
            },
            {
                "Common.OpenFolder.tip",
                new[]
                {
                    "탐색기에서 해당 폴더를 엽니다.",
                    "Reveal this folder in the file explorer.",
                    "エクスプローラーでこのフォルダを開きます。"
                }
            },
            {
                "Common.Ping",
                new[]
                {
                    "프로젝트에서 보기",
                    "Show in Project",
                    "プロジェクトで表示"
                }
            },
            {
                "Common.Ping.tip",
                new[]
                {
                    "Project 창에서 이 에셋을 강조 표시합니다.",
                    "Highlight this asset in the Project window.",
                    "Projectウィンドウでこのアセットをハイライトします。"
                }
            },
            {
                "Common.TypeProp",
                new[]
                {
                    "소품 (Prop)",
                    "Prop",
                    "小物 (Prop)"
                }
            },
            {
                "Common.TypeCharacter",
                new[]
                {
                    "캐릭터 (Character)",
                    "Character",
                    "キャラ (Character)"
                }
            },
            {
                "Common.TypeEnvironment",
                new[]
                {
                    "환경 (Environment)",
                    "Environment",
                    "環境 (Environment)"
                }
            },
            {
                "Common.StatusPending",
                new[]
                {
                    "대기",
                    "Pending",
                    "待機"
                }
            },
            {
                "Common.StatusRunning",
                new[]
                {
                    "진행 중",
                    "Building",
                    "実行中"
                }
            },
            {
                "Common.StatusSuccess",
                new[]
                {
                    "성공",
                    "Success",
                    "成功"
                }
            },
            {
                "Common.StatusFailed",
                new[]
                {
                    "실패",
                    "Failed",
                    "失敗"
                }
            },
            {
                "Common.StatusCanceled",
                new[]
                {
                    "중단됨",
                    "Canceled",
                    "中断"
                }
            },
            {
                "Common.StatusSkipped",
                new[]
                {
                    "건너뜀",
                    "Skipped",
                    "スキップ"
                }
            },
            {
                "Common.Preset",
                new[]
                {
                    "프리셋",
                    "Preset",
                    "プリセット"
                }
            },
            {
                "Common.PresetAutoName",
                new[]
                {
                    "프리셋 {0}",
                    "Preset {0}",
                    "プリセット {0}"
                }
            },
            {
                "Common.Rebuild",
                new[]
                {
                    "재빌드",
                    "Rebuild",
                    "再ビルド"
                }
            },
            {
                "Common.Rebuild.tip",
                new[]
                {
                    "이 기록의 설정을 그대로 복원해서 다시 빌드합니다.",
                    "Restore this record's settings and build it again.",
                    "この記録の設定を復元して再度ビルドします。"
                }
            },
            {
                "Common.Sort",
                new[]
                {
                    "정렬",
                    "Sort",
                    "並び替え"
                }
            },
            {
                "Common.SortName",
                new[]
                {
                    "이름",
                    "Name",
                    "名前"
                }
            },
            {
                "Common.SortDate",
                new[]
                {
                    "최신순",
                    "Newest",
                    "新しい順"
                }
            },
            {
                "Common.SortSize",
                new[]
                {
                    "크기순",
                    "Size",
                    "サイズ順"
                }
            },
            {
                "Win.BusyRebuild",
                new[]
                {
                    "빌드가 진행 중입니다. 끝난 뒤에 다시 시도하세요.",
                    "A build is already running. Try again after it finishes.",
                    "ビルドが進行中です。終了後にもう一度お試しください。"
                }
            },
            {
                "Common.NoMatch",
                new[]
                {
                    "검색 조건에 맞는 항목이 없습니다. ({0}개 중 0개)",
                    "No item matches the search. (0 of {0})",
                    "検索条件に一致する項目がありません。({0}件中 0件)"
                }
            },
        };
    }
}
