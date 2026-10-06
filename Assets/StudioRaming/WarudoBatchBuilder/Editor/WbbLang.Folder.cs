using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> FolderTexts = new Dictionary<string, string[]>
        {
            {
                "Folder.Browse",
                new[]
                {
                    "폴더 찾기",
                    "Browse",
                    "参照"
                }
            },
            {
                "Folder.Browse.tip",
                new[]
                {
                    "프로젝트 폴더 트리를 열어서 폴더를 고릅니다. 이름으로 검색할 수도 있습니다.",
                    "Open the project folder tree and pick a folder. You can also search by name.",
                    "プロジェクトのフォルダツリーを開いて選びます。名前で検索もできます。"
                }
            },
            {
                "Folder.Ping",
                new[]
                {
                    "보기",
                    "Show",
                    "表示"
                }
            },
            {
                "Folder.Ping.tip",
                new[]
                {
                    "Project 창에서 이 폴더를 강조 표시합니다.",
                    "Highlight this folder in the Project window.",
                    "Projectウィンドウでこのフォルダをハイライトします。"
                }
            },
            {
                "Folder.NotSet",
                new[]
                {
                    "폴더가 아직 선택되지 않았습니다. 아래로 폴더를 끌어다 놓거나 [폴더 찾기]를 누르세요.",
                    "No folder selected yet. Drop a folder below, or press [Browse].",
                    "フォルダが未選択です。下にフォルダをドロップするか [参照] を押してください。"
                }
            },
            {
                "Folder.InvalidPath",
                new[]
                {
                    "존재하지 않는 폴더입니다: {0}",
                    "Folder does not exist: {0}",
                    "存在しないフォルダです: {0}"
                }
            },
            {
                "Folder.DropHint",
                new[]
                {
                    "Project 창에서 폴더를 여기로 끌어다 놓으세요",
                    "Drag a folder here from the Project window",
                    "Projectウィンドウからフォルダをここにドラッグ"
                }
            },
            {
                "Folder.Recent",
                new[]
                {
                    "최근",
                    "Recent",
                    "最近"
                }
            },
            {
                "Folder.Recent.tip",
                new[]
                {
                    "최근에 사용한 폴더입니다. 한 번 클릭하면 바로 전환됩니다.",
                    "Recently used folders. One click switches to it.",
                    "最近使ったフォルダです。クリックですぐ切り替わります。"
                }
            },
            {
                "Folder.RecentClear",
                new[]
                {
                    "목록 지우기",
                    "Clear",
                    "履歴クリア"
                }
            },
            {
                "Folder.RecentClear.tip",
                new[]
                {
                    "이 항목의 최근 폴더 목록을 비웁니다. 폴더 자체는 지워지지 않습니다.",
                    "Clear the recent folder list for this field. The folders themselves are not touched.",
                    "この項目の最近使ったフォルダ一覧を消去します。フォルダ自体は削除されません。"
                }
            },
            {
                "Folder.RecentEmpty",
                new[]
                {
                    "최근에 사용한 폴더가 없습니다.",
                    "No recent folders yet.",
                    "最近使ったフォルダはありません。"
                }
            },
            {
                "Folder.RootMissing",
                new[]
                {
                    "먼저 폴더를 선택하세요.",
                    "Select a folder first.",
                    "先にフォルダを選択してください。"
                }
            },
            {
                "Folder.Up",
                new[]
                {
                    "상위 폴더",
                    "Up",
                    "上の階層"
                }
            },
            {
                "Folder.Up.tip",
                new[]
                {
                    "한 단계 위 폴더로 이동합니다.",
                    "Go up one folder level.",
                    "一つ上のフォルダへ移動します。"
                }
            },
            {
                "Folder.CurrentIs",
                new[]
                {
                    "현재: {0}",
                    "Current: {0}",
                    "現在: {0}"
                }
            },
            {
                "Folder.Siblings",
                new[]
                {
                    "형제 폴더 {0}개: 클릭하면 바로 전환",
                    "Sibling folders ({0}): click to switch",
                    "兄弟フォルダ {0}件: クリックで切り替え"
                }
            },
            {
                "Folder.Children",
                new[]
                {
                    "하위 폴더 {0}개",
                    "Subfolders ({0})",
                    "サブフォルダ {0}件"
                }
            },
            {
                "Folder.NoChildren",
                new[]
                {
                    "하위 폴더가 없습니다.",
                    "No subfolders.",
                    "サブフォルダはありません。"
                }
            },
            {
                "Folder.BrowserTitle",
                new[]
                {
                    "폴더 선택",
                    "Select Folder",
                    "フォルダ選択"
                }
            },
            {
                "Folder.SearchHint",
                new[]
                {
                    "폴더 검색",
                    "Find folder",
                    "フォルダ検索"
                }
            },
            {
                "Folder.SearchHint.tip",
                new[]
                {
                    "이름 일부를 입력하면 트리 대신 일치하는 폴더 목록이 나옵니다.",
                    "Type part of a name to list matching folders instead of the tree.",
                    "名前の一部を入力すると、ツリーの代わりに一致するフォルダ一覧を表示します。"
                }
            },
            {
                "Folder.SearchResult",
                new[]
                {
                    "검색 결과 {0}개",
                    "{0} matching folders",
                    "検索結果 {0}件"
                }
            },
            {
                "Folder.SearchTruncated",
                new[]
                {
                    "결과가 많아 상위 {0}개만 표시합니다. 검색어를 더 입력하세요.",
                    "Too many results. Showing the first {0}. Type more to narrow it down.",
                    "結果が多いため上位 {0}件のみ表示します。検索語を追加してください。"
                }
            },
            {
                "Folder.SearchNoMatch",
                new[]
                {
                    "일치하는 폴더가 없습니다.",
                    "No folder matches.",
                    "一致するフォルダがありません。"
                }
            },
            {
                "Tree.Title",
                new[]
                {
                    "① 폴더",
                    "① Folders",
                    "① フォルダ"
                }
            },
            {
                "Tree.Title.tip",
                new[]
                {
                    "Project 창처럼 폴더를 골라 그 안의 프리팹을 봅니다. 클릭 한 번으로 전환됩니다.",
                    "Pick a folder like in the Project window to list its prefabs. One click switches.",
                    "Projectウィンドウのようにフォルダを選んで中のプレハブを表示します。クリック1回で切り替わります。"
                }
            },
            {
                "Tree.OnlyPrefabs",
                new[]
                {
                    "◇",
                    "◇",
                    "◇"
                }
            },
            {
                "Tree.OnlyPrefabs.tip",
                new[]
                {
                    "프리팹이 들어 있는 폴더만 보여줍니다. 끄면 프로젝트의 모든 폴더가 나옵니다.\n폴더 이름 뒤 괄호는 그 폴더 바로 아래의 프리팹 개수입니다.",
                    "Show only folders that contain prefabs. Turn it off to list every folder.\nThe number in parentheses is how many prefabs sit directly in that folder.",
                    "プレハブが入っているフォルダのみ表示します。オフにするとすべてのフォルダが出ます。\nフォルダ名の後の括弧はそのフォルダ直下のプレハブ数です。"
                }
            },
            {
                "Tree.Collapse",
                new[]
                {
                    "⊟",
                    "⊟",
                    "⊟"
                }
            },
            {
                "Tree.Collapse.tip",
                new[]
                {
                    "펼친 폴더를 모두 접습니다.",
                    "Collapse all expanded folders.",
                    "展開したフォルダをすべて折りたたみます。"
                }
            },
            {
                "Tree.SetFromSelection",
                new[]
                {
                    "지정",
                    "Set",
                    "設定"
                }
            },
            {
                "Folder.UseProjectSelection",
                new[]
                {
                    "Project 선택 사용",
                    "Use Project selection",
                    "Project の選択を使用"
                }
            },
            {
                "Folder.UseProjectSelection.tip",
                new[]
                {
                    "Project 창에서 지금 선택 중인 폴더를 가져옵니다.",
                    "Take the folder currently selected in the Project window.",
                    "Projectウィンドウで現在選択中のフォルダを取り込みます。"
                }
            },
            {
                "Folder.SelectionNotFolder",
                new[]
                {
                    "Project 창에서 선택한 항목이 폴더가 아닙니다.",
                    "The current Project selection is not a folder.",
                    "Projectウィンドウで選択中の項目はフォルダではありません。"
                }
            },
            {
                "Folder.NoSelection",
                new[]
                {
                    "선택된 폴더가 없습니다.",
                    "No folder selected.",
                    "フォルダが選択されていません。"
                }
            },
            {
                "Folder.Select",
                new[]
                {
                    "이 폴더 선택",
                    "Select",
                    "このフォルダを選択"
                }
            },
            {
                "Folder.Select.tip",
                new[]
                {
                    "강조된 폴더를 선택하고 창을 닫습니다. (목록에서 더블클릭해도 같습니다)",
                    "Pick the highlighted folder and close this window. (Double-clicking a row does the same)",
                    "選択中のフォルダを確定してウィンドウを閉じます。（行のダブルクリックでも同じ）"
                }
            },
        };
    }
}
