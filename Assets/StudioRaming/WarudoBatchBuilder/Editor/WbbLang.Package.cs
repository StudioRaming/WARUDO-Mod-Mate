using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> PackageTexts = new Dictionary<string, string[]>
        {
            {
                "Package.RiskExcludedSummary",
                new[]
                {
                    "재배포 주의 파일 {0}개 제외",
                    "Redistribution caution: {0} files excluded",
                    "再配布注意: {0} ファイルを除外"
                }
            },
            {
                "Package.RiskIncludedSummary",
                new[]
                {
                    "재배포 주의 파일 {0}개 포함 · {1}개 제외",
                    "Redistribution caution: {0} included · {1} excluded",
                    "再配布注意: {0} 個を含む・{1} 個を除外"
                }
            },
            {
                "Package.RiskMore",
                new[]
                {
                    "자세히 ▾",
                    "Details ▾",
                    "詳細 ▾"
                }
            },
            {
                "Package.RiskLess",
                new[]
                {
                    "접기 ▴",
                    "Hide ▴",
                    "閉じる ▴"
                }
            },
            {
                "Package.TargetsTitle",
                new[]
                {
                    "내보낼 프리팹",
                    "Prefabs to export",
                    "書き出すプレハブ"
                }
            },
            {
                "Package.TargetsTitle.tip",
                new[]
                {
                    "프리팹 하나가 .unitypackage 하나가 됩니다. 여러 개를 넣으면 버튼 한 번으로 순서대로 전부 만들어집니다.",
                    "One prefab becomes one .unitypackage. Add several and a single click builds them all, one after another.",
                    "プレハブ1つが .unitypackage 1つになります。複数入れればボタン1回で順番にすべて作成します。"
                }
            },
            {
                "Package.TargetsEmpty",
                new[]
                {
                    "아직 대상이 없습니다. 빌드 탭에서 프리팹을 고른 뒤 .unitypackage 로 내보내거나, Project 창에서 프리팹·폴더를 위 상자로 끌어다 놓으세요.",
                    "No target yet. Pick prefabs in the Build tab and export them as .unitypackage, or drag prefabs/folders from the Project window onto the box above.",
                    "対象がまだありません。ビルドタブでプレハブを選んで .unitypackage で書き出すか、Projectウィンドウからプレハブ・フォルダを上のボックスにドラッグしてください。"
                }
            },
            {
                "Package.DropHint",
                new[]
                {
                    "여기로 프리팹 또는 폴더를 끌어다 놓으세요 (폴더는 안에 있는 프리팹을 모두 가져옵니다)",
                    "Drag prefabs or folders here (a folder brings in every prefab inside it)",
                    "ここにプレハブまたはフォルダをドラッグ（フォルダ内のプレハブをすべて取り込みます）"
                }
            },
            {
                "Package.AddSelection",
                new[]
                {
                    "Project 선택 추가",
                    "Add Project selection",
                    "Project の選択を追加"
                }
            },
            {
                "Package.AddSelection.tip",
                new[]
                {
                    "Project 창에서 지금 선택 중인 프리팹을 대상에 추가합니다. 폴더를 선택했다면 그 안의 프리팹을 전부 가져옵니다.",
                    "Add the prefabs currently selected in the Project window. Selecting a folder brings in every prefab inside it.",
                    "Projectウィンドウで現在選択中のプレハブを対象に追加します。フォルダを選ぶと中のプレハブをすべて取り込みます。"
                }
            },
            {
                "Package.ClearTargets",
                new[]
                {
                    "목록 비우기",
                    "Clear list",
                    "リストを空にする"
                }
            },
            {
                "Package.ClearTargets.tip",
                new[]
                {
                    "대상 목록만 비웁니다. 프리팹 파일과 제외 기억은 그대로입니다.",
                    "Empty the target list only. Your prefab files and remembered exclusions are untouched.",
                    "対象リストのみ空にします。プレハブファイルと除外の記憶はそのままです。"
                }
            },
            {
                "Package.RemoveTarget.tip",
                new[]
                {
                    "이 프리팹을 대상에서 뺍니다. 파일은 지워지지 않습니다.",
                    "Remove this prefab from the targets. The file itself is not deleted.",
                    "このプレハブを対象から外します。ファイルは削除されません。"
                }
            },
            {
                "Package.TargetSummary",
                new[]
                {
                    "프리팹 {0}개 → .unitypackage {0}개",
                    "{0} prefab(s) → {0} .unitypackage file(s)",
                    "プレハブ {0}件 → .unitypackage {0}件"
                }
            },
            {
                "Package.TargetTip",
                new[]
                {
                    "{0}\n경로: {1}\n만들어질 파일: {2}",
                    "{0}\nPath: {1}\nWill be written as: {2}",
                    "{0}\nパス: {1}\n作成されるファイル: {2}"
                }
            },
            {
                "Package.TargetMissing",
                new[]
                {
                    "(파일 없음)",
                    "(file missing)",
                    "(ファイルなし)"
                }
            },
            {
                "Package.OutputTitle",
                new[]
                {
                    "저장 폴더",
                    "Output folder",
                    "保存フォルダ"
                }
            },
            {
                "Package.OutputTitle.tip",
                new[]
                {
                    ".unitypackage 파일이 저장될 폴더입니다. 프로젝트 밖이어도 됩니다. 기본값은 프로젝트 폴더의 부모입니다.",
                    "Where the .unitypackage files are written. It can be outside the project. Defaults to the project's parent folder.",
                    ".unitypackage を保存するフォルダです。プロジェクト外でも構いません。既定はプロジェクトフォルダの親です。"
                }
            },
            {
                "Package.OutputNotSet",
                new[]
                {
                    "(저장 폴더 미지정)",
                    "(output folder not set)",
                    "(保存フォルダ未設定)"
                }
            },
            {
                "Package.Browse",
                new[]
                {
                    "…",
                    "…",
                    "…"
                }
            },
            {
                "Package.Browse.tip",
                new[]
                {
                    "저장 폴더를 고릅니다.",
                    "Pick the output folder.",
                    "保存フォルダを選びます。"
                }
            },
            {
                "Package.BrowseTitle",
                new[]
                {
                    "패키지를 저장할 폴더 선택",
                    "Select the output folder",
                    "パッケージの保存フォルダを選択"
                }
            },
            {
                "Package.OutputEmpty",
                new[]
                {
                    "저장 폴더가 비어 있습니다. 먼저 폴더를 지정하세요.",
                    "The output folder is empty. Pick a folder first.",
                    "保存フォルダが空です。先にフォルダを指定してください。"
                }
            },
            {
                "Package.OutputMissing",
                new[]
                {
                    "저장 폴더가 없습니다:\n{0}\n\n지금 만들까요?",
                    "The output folder does not exist:\n{0}\n\nCreate it now?",
                    "保存フォルダがありません:\n{0}\n\n今すぐ作成しますか？"
                }
            },
            {
                "Package.OutputCreateYes",
                new[]
                {
                    "폴더 만들기",
                    "Create Folder",
                    "フォルダを作成"
                }
            },
            {
                "Package.OutputOpenOff.tip",
                new[]
                {
                    "저장 폴더가 비어 있어 꺼져 있습니다. 경로를 적거나 … 버튼으로 폴더를 고르면 켜집니다.",
                    "Off because the output folder is empty. Type a path or pick a folder with the … button to turn it on.",
                    "保存フォルダが空のためオフです。パスを入力するか … ボタンでフォルダを選ぶとオンになります。"
                }
            },
            {
                "Package.OutputCreateFailed",
                new[]
                {
                    "저장 폴더를 만들 수 없습니다: {0}",
                    "Cannot create the output folder: {0}",
                    "保存フォルダを作成できません: {0}"
                }
            },
            {
                "Package.GatherTitle",
                new[]
                {
                    "한 폴더로 모으기",
                    "Gather into one folder",
                    "1つのフォルダにまとめる"
                }
            },
            {
                "Package.GatherTitle.tip",
                new[]
                {
                    "맷캡·노멀맵처럼 여러 의상이 같이 쓰는 파일을 패키지 안 한 폴더(<패키지>/Textures 등)로 모읍니다.\n끈 종류는 원래 폴더 구조대로 들어갑니다. 연결은 그대로 유지되고 원본 프로젝트는 바뀌지 않습니다.\n이름이 겹치면 _2 가 붙고, Resources 폴더 안의 파일은 옮기지 않습니다.",
                    "Gathers files that several outfits share (matcaps, normal maps) into one folder in the package (<package>/Textures etc.).\nTypes left off keep their original folders. Links stay intact and your project is not changed.\nDuplicate names get _2; files under a Resources folder are never moved.",
                    "マットキャップやノーマルマップなど共有ファイルを、パッケージ内の1フォルダ（<パッケージ>/Textures など）にまとめます。\nオフの種類は元のフォルダ構造のまま入ります。参照は保たれ、元のプロジェクトは変わりません。\n名前が重複すると _2 が付き、Resources フォルダ内のファイルは移動しません。"
                }
            },
            {
                "Package.GatherTex",
                new[]
                {
                    "텍스처",
                    "Textures",
                    "テクスチャ"
                }
            },
            {
                "Package.GatherTex.tip",
                new[]
                {
                    "텍스처(.png/.tga/.psd/.exr …)를 <패키지이름>/Textures/ 한 곳에 모읍니다.\n맷캡·노멀맵처럼 여러 의상이 돌려 쓰는 파일이 흩어지지 않습니다.",
                    "Gathers textures (.png/.tga/.psd/.exr …) into a single <package name>/Textures/.\nKeeps shared files such as matcaps and normal maps from scattering.",
                    "テクスチャ（.png/.tga/.psd/.exr …）を <パッケージ名>/Textures/ にまとめます。\nマットキャップやノーマルマップのように使い回すファイルが散らばりません。"
                }
            },
            {
                "Package.GatherMat",
                new[]
                {
                    "머티리얼",
                    "Materials",
                    "マテリアル"
                }
            },
            {
                "Package.GatherMat.tip",
                new[]
                {
                    "머티리얼(.mat)을 <패키지이름>/Materials/ 한 곳에 모읍니다.\n앞머리·뒷머리처럼 폴더가 갈려 있던 머티리얼을 한눈에 보고 고칠 수 있습니다.",
                    "Gathers materials (.mat) into a single <package name>/Materials/.\nMaterials that were split across folders end up in one place, easy to review and tweak.",
                    "マテリアル（.mat）を <パッケージ名>/Materials/ にまとめます。\nフォルダが分かれていたマテリアルを一目で見て調整できます。"
                }
            },
            {
                "Package.GatherFbx",
                new[]
                {
                    "FBX",
                    "FBX",
                    "FBX"
                }
            },
            {
                "Package.GatherFbx.tip",
                new[]
                {
                    "모델 파일(.fbx/.obj/.blend …)을 <패키지이름>/FBX/ 한 곳에 모읍니다.\n모델 안의 메시·본 연결은 에셋 고유 번호로 이어져 있어 위치가 바뀌어도 그대로입니다.",
                    "Gathers model files (.fbx/.obj/.blend …) into a single <package name>/FBX/.\nMesh and bone links inside the model follow each asset's unique ID, so the move changes nothing.",
                    "モデルファイル（.fbx/.obj/.blend …）を <パッケージ名>/FBX/ にまとめます。\nモデル内のメッシュ・ボーンの接続はアセット固有の番号で繋がっているため、移動しても変わりません。"
                }
            },
            {
                "Package.GatherPrefab",
                new[]
                {
                    "프리팹",
                    "Prefabs",
                    "プレハブ"
                }
            },
            {
                "Package.GatherPrefab.tip",
                new[]
                {
                    "프리팹(.prefab)을 <패키지이름>/Prefabs/ 한 곳에 모읍니다.\n직접 고른 루트 프리팹도 함께 모입니다. 받는 쪽에서 무엇을 끌어다 쓰면 되는지 바로 보입니다.",
                    "Gathers prefabs (.prefab) into a single <package name>/Prefabs/.\nThe root prefabs you picked go there too, so whoever imports it sees at once what to drag in.",
                    "プレハブ（.prefab）を <パッケージ名>/Prefabs/ にまとめます。\n自分で選んだルートプレハブもそこに入るため、受け取った側が何をドラッグすればよいか一目で分かります。"
                }
            },
            {
                "Package.SearchHint",
                new[]
                {
                    "에셋 검색",
                    "Find asset",
                    "アセット検索"
                }
            },
            {
                "Package.SearchHint.tip",
                new[]
                {
                    "에셋 이름이나 경로의 일부로 목록을 거릅니다. 공백으로 나눈 단어는 모두 들어간 항목만 남고, 그 상위 폴더는 자동으로 펼쳐집니다.\n[전체 선택]·[전체 해제]와 폴더 체크는 걸러진 항목에만 적용됩니다.",
                    "Filters by part of an asset name or path. Space-separated words must all match; parent folders open automatically.\n[Select All] / [Clear Selection] and folder checkboxes affect filtered items only.",
                    "アセット名やパスの一部でリストを絞り込みます。スペース区切りの語はすべて含む項目のみ残り、親フォルダは自動的に展開されます。\n[全選択]・[全解除]とフォルダのチェックは絞り込み後の項目にのみ適用されます。"
                }
            },
            {
                "Package.EmptyTargetsTitle",
                new[]
                {
                    "내보낼 프리팹이 없습니다",
                    "No prefab to export",
                    "書き出すプレハブがありません"
                }
            },
            {
                "Package.DepsEmpty",
                new[]
                {
                    "대상 프리팹을 먼저 추가하면, 그 프리팹이 쓰는 에셋이 원래 폴더 구조 그대로 여기에 나옵니다.",
                    "Add a target prefab first. Everything it uses will be listed here in its original folder structure.",
                    "先に対象プレハブを追加すると、そのプレハブが使うアセットが元のフォルダ構造のままここへ表示されます。"
                }
            },
            {
                "Package.EmptyTargetsEmbedded",
                new[]
                {
                    "위 목록에서 프리팹을 체크하면, 그 프리팹이 쓰는 에셋이 여기에 나옵니다.",
                    "Check prefabs in the list above to see the assets they use here.",
                    "上の一覧でプレハブにチェックを入れると、使っているアセットがここに表示されます。"
                }
            },
            {
                "Package.EmptyAssetsTitle",
                new[]
                {
                    "포함할 에셋이 없습니다",
                    "No asset to include",
                    "含めるアセットがありません"
                }
            },
            {
                "Package.DepsNone",
                new[]
                {
                    "프리팹이 삭제되었거나 Unity 내장 리소스만 쓰고 있을 수 있습니다. 고친 뒤 새로고침하세요.",
                    "The prefab may have been deleted, or it only uses Unity built-in resources. Fix it, then refresh.",
                    "プレハブが削除されたか、Unity内蔵リソースのみを使用している可能性があります。直してから更新してください。"
                }
            },
            {
                "Package.GroupInfo",
                new[]
                {
                    "{0}개 · {1}",
                    "{0} items · {1}",
                    "{0}件 · {1}"
                }
            },
            {
                "Package.FolderToggle.tip",
                new[]
                {
                    "원래 프로젝트에 있던 폴더입니다. 패키지 안에서도 이 구조 그대로 들어갑니다.\n왼쪽 체크박스로 이 폴더 아래 전부를 한 번에 포함/제외할 수 있고, 이름을 누르면 접거나 펼칩니다.\n재배포 주의 파일은 폴더 체크로 켜지지 않습니다(각 행을 직접 체크). 끌 때는 함께 꺼집니다.\n일부만 포함되어 있으면 체크박스가 중간 상태로 보입니다. 꺼진 재배포 주의 파일과 직접 고른 루트 프리팹(꺼지지 않음)은 이 판정에서 빼고 봅니다.",
                    "This is the folder the assets already live in. The package keeps the very same structure.\nThe checkbox on the left includes/excludes everything under this folder at once; clicking the name folds it.\nRedistribution-risk files are never turned on by the folder checkbox (tick each row yourself), but turning the folder off turns them off too.\nA half-filled checkbox means only part of it is included. Excluded redistribution-risk files and root prefabs you picked yourself (never turned off) are left out of that check.",
                    "元のプロジェクトにあるフォルダです。パッケージ内でもこの構造のまま入ります。\n左のチェックでこのフォルダ以下を一括で含める/除外でき、名前をクリックすると折りたたみます。\n再配布注意のファイルはフォルダのチェックでは含まれません（各行を直接チェック）。外す時は一緒に外れます。\n一部だけ含まれている場合はチェックが中間状態になります。外れている再配布注意のファイルと自分で選んだルートプレハブ（外れません）はこの判定から除きます。"
                }
            },
            {
                "Package.FolderLocked.tip",
                new[]
                {
                    "이 폴더에는 폴더 체크로 바꿀 수 있는 항목이 없습니다.\n직접 고른 루트 프리팹은 뺄 수 없고, 재배포 주의 파일은 각 행을 직접 체크해야 들어갑니다.",
                    "Nothing in this folder can be changed with the folder checkbox.\nRoot prefabs you picked yourself cannot be unchecked, and redistribution-risk files go in only when you tick each row.",
                    "このフォルダにはフォルダのチェックで変更できる項目がありません。\n自分で選んだルートプレハブは外せず、再配布注意のファイルは各行を直接チェックした時だけ含まれます。"
                }
            },
            {
                "Package.SelectAll.tip",
                new[]
                {
                    "지금 목록에 보이는 항목을 전부 포함합니다(검색 중이면 걸러진 항목만).\n재배포 주의 파일은 켜지 않습니다. 넣으려면 각 행을 직접 체크하세요. [전체 해제]는 그것까지 끕니다.",
                    "Include every item shown in the list (only the filtered ones while searching).\nRedistribution-risk files are not turned on. Tick each of those rows yourself. [Clear Selection] turns them off too.",
                    "今リストに表示されている項目をすべて含めます（検索中は絞り込まれた項目のみ）。\n再配布注意のファイルはオンにしません。含めるには各行を直接チェックしてください。[全解除]はそれも外します。"
                }
            },
            {
                "Package.ExpandAll.tip",
                new[]
                {
                    "폴더를 전부 펼칩니다.",
                    "Expand every folder.",
                    "すべてのフォルダを展開します。"
                }
            },
            {
                "Package.CollapseAll.tip",
                new[]
                {
                    "폴더를 전부 접습니다. 접힘 상태는 다음에 창을 열 때도 그대로 유지됩니다.",
                    "Collapse every folder. The folded state is kept the next time you open this window.",
                    "すべてのフォルダを折りたたみます。折りたたみ状態は次回この画面を開いた時も保たれます。"
                }
            },
            {
                "Package.RootBadge",
                new[]
                {
                    "루트",
                    "Root",
                    "ルート"
                }
            },
            {
                "Package.RootLocked.tip",
                new[]
                {
                    "직접 고른 프리팹이라 뺄 수 없습니다. 이 항목이 빠지면 패키지를 만드는 의미가 없습니다.\n대상에서 빼려면 위쪽 프리팹 목록에서 그 프리팹의 선택을 해제하세요.",
                    "This is a prefab you picked yourself, so it cannot be unchecked. Without it the package would be pointless.\nTo drop it, deselect that prefab in the prefab list above.",
                    "自分で選んだプレハブなので外せません。これが抜けるとパッケージを作る意味がなくなります。\n対象から外すには上のプレハブ一覧でそのプレハブの選択を解除してください。"
                }
            },
            {
                "Package.StateIncluded",
                new[]
                {
                    "포함됨",
                    "Included",
                    "含む"
                }
            },
            {
                "Package.StateExcluded",
                new[]
                {
                    "제외됨 (기억됨: 다음 내보내기에서도 해제 상태)",
                    "Excluded (remembered: stays off for the next export too)",
                    "除外 (記憶済み: 次回の書き出しでも外れたまま)"
                }
            },
            {
                "Package.StateRiskExcluded",
                new[]
                {
                    "제외됨 (재배포 주의: 기본 제외. 이 행을 체크하면 넣은 것으로 기억됩니다)",
                    "Excluded (redistribution risk: left out by default. Tick this row and that choice is remembered)",
                    "除外 (再配布注意: 既定で除外。この行をチェックすると含めたことが記憶されます)"
                }
            },
            {
                "Package.RiskExcludedNotice",
                new[]
                {
                    "재배포 주의 파일 {0}개(스크립트 폴더 밖의 코드 · 셰이더 · DLL)는 기본으로 빠져 있습니다. 직접 만든 파일이면 그 행을 하나씩 체크해서 넣으세요([전체 선택] · 폴더 체크로는 들어가지 않습니다).",
                    "{0} redistribution-risk file(s) (code, shaders or DLLs outside your script folder) are left out by default. Tick the rows of the ones you made yourself ([Select All] and folder checkboxes skip them).",
                    "再配布注意のファイル {0}件（スクリプトフォルダ外のコード・シェーダー・DLL）は既定で除外しています。自作のものは各行をチェックして含めてください（[全選択]・フォルダのチェックでは含まれません）。"
                }
            },
            {
                "Package.RiskIncludedNotice",
                new[]
                {
                    "재배포 주의 파일 {0}개가 패키지에 들어갑니다. 다른 사람이 만든 코드 · 셰이더 · DLL 이면 체크를 해제하세요.",
                    "{0} redistribution-risk file(s) will go into the package. Untick any code, shader or DLL made by someone else.",
                    "再配布注意のファイル {0}件がパッケージに入ります。他者が作ったコード・シェーダー・DLLならチェックを外してください。"
                }
            },
            {
                "Package.ItemTip",
                new[]
                {
                    "{0}\n경로: {1}\n패키지 안 폴더: <패키지이름>/{2}/\n크기: {3}\n상태: {4}",
                    "{0}\nPath: {1}\nFolder inside the package: <package name>/{2}/\nSize: {3}\nState: {4}",
                    "{0}\nパス: {1}\nパッケージ内のフォルダ: <パッケージ名>/{2}/\nサイズ: {3}\n状態: {4}"
                }
            },
            {
                "Package.IncludedSummary",
                new[]
                {
                    "포함 {0} / {1}개 · {2}",
                    "{0} of {1} included · {2}",
                    "含む {0} / {1}件 · {2}"
                }
            },
            {
                "Package.ExcludedCount",
                new[]
                {
                    "기억된 제외 {0}개",
                    "{0} remembered exclusions",
                    "記憶された除外 {0}件"
                }
            },
            {
                "Package.ExcludedCount.tip",
                new[]
                {
                    "체크를 해제한 에셋은 기억되어, 다음에 다른 프리팹을 내보낼 때도 해제된 채로 뜹니다. 파일을 옮기거나 이름을 바꿔도 기억은 그대로입니다.",
                    "Assets you uncheck are remembered and stay unchecked when you export a different prefab later, even if the file is moved or renamed.",
                    "チェックを外したアセットは記憶され、次に別のプレハブを書き出す時も外れたまま表示されます。ファイルを移動・改名しても記憶は保たれます。"
                }
            },
            {
                "Package.ClearExcluded",
                new[]
                {
                    "제외 기억 비우기",
                    "Clear exclusions",
                    "除外の記憶を消す"
                }
            },
            {
                "Package.ClearExcluded.tip",
                new[]
                {
                    "체크 해제해 둔 기억과, 재배포 주의 파일을 직접 다시 넣은 기억을 전부 지웁니다. 처음 상태로 돌아갑니다: 의존 에셋은 다시 포함, 재배포 주의 파일은 다시 기본 제외.",
                    "Forget every asset you unchecked and every redistribution-risk file you checked back in. Back to the start: dependencies are included again and risk files are left out again by default.",
                    "チェックを外した記憶と、再配布注意のファイルを手動で入れ直した記憶をすべて消します。最初の状態に戻ります: 依存アセットは再び含まれ、再配布注意のファイルは再び既定で除外されます。"
                }
            },
            {
                "Package.ClearExcludedTitle",
                new[]
                {
                    "제외 기억 비우기",
                    "Clear remembered exclusions",
                    "除外の記憶を消す"
                }
            },
            {
                "Package.ClearExcludedConfirm",
                new[]
                {
                    "제외 기억 {0}개와 재배포 주의 다시 넣기 기억 {1}개를 모두 지웁니다.\n의존 에셋은 다시 전부 포함되고, 직접 다시 넣었던 재배포 주의 파일은 다시 빠집니다.\n\n계속할까요?",
                    "This clears {0} remembered exclusions and {1} redistribution-risk files you checked back in.\nEvery dependency is included again, and those risk files are left out again.\n\nContinue?",
                    "除外の記憶 {0}件と、再配布注意を入れ直した記憶 {1}件をすべて消します。\n依存アセットは再びすべて含まれ、入れ直した再配布注意のファイルは再び外れます。\n\n続けますか？"
                }
            },
            {
                "Package.ClearExcludedYes",
                new[]
                {
                    "비우기",
                    "Clear",
                    "消す"
                }
            },
            {
                "Package.ClearExcludedNone",
                new[]
                {
                    "기억된 제외가 없습니다.",
                    "There is nothing to clear.",
                    "記憶された除外はありません。"
                }
            },
            {
                "Package.ClearExcludedDone",
                new[]
                {
                    "제외 기억 {0}개, 재배포 주의 다시 넣기 기억 {1}개를 지웠습니다.",
                    "Cleared {0} remembered exclusions and {1} risk re-includes.",
                    "除外の記憶 {0}件と、再配布注意の入れ直し {1}件を消しました。"
                }
            },
            {
                "Package.SelectionNoPrefab",
                new[]
                {
                    "프리팹을 찾지 못했습니다. 프리팹 파일이나 프리팹이 들어 있는 폴더를 넣어 주세요.",
                    "No prefab found. Drop a prefab file, or a folder that contains prefabs.",
                    "プレハブが見つかりません。プレハブファイル、またはプレハブが入っているフォルダを入れてください。"
                }
            },
            {
                "Package.AddedCount",
                new[]
                {
                    "프리팹 {0}개를 대상에 추가했습니다. (이미 들어 있던 항목은 건너뜁니다)",
                    "Added {0} prefab(s) to the targets. (duplicates are skipped)",
                    "プレハブ {0}件を対象に追加しました。（重複はスキップされます）"
                }
            },
            {
                "Package.PingMissing",
                new[]
                {
                    "에셋을 찾을 수 없습니다: {0}",
                    "Asset not found: {0}",
                    "アセットが見つかりません: {0}"
                }
            },
            {
                "Package.Export",
                new[]
                {
                    "{0}개 내보내기",
                    "Export {0}",
                    "{0}件を書き出す"
                }
            },
            {
                "Package.Export.tip",
                new[]
                {
                    "대상 프리팹마다 .unitypackage 를 하나씩, 위에서부터 순서대로 만듭니다. 진행 중에 취소하면 그때까지 만들어진 파일은 남습니다.",
                    "Builds one .unitypackage per target prefab, top to bottom. Cancelling mid-run keeps whatever was already written.",
                    "対象プレハブごとに .unitypackage を上から順に1つずつ作成します。途中で中止しても、それまでに作成されたファイルは残ります。"
                }
            },
            {
                "Package.ExportOffNoTargets.tip",
                new[]
                {
                    "내보낼 프리팹이 없어 꺼져 있습니다. 위 상자에 프리팹을 끌어다 놓거나 [Project 선택 추가]를 누르면 켜집니다.",
                    "Off because there is no prefab to export. Drag prefabs onto the box above or press [Add Project selection] to turn it on.",
                    "書き出すプレハブがないためオフです。上のボックスにプレハブをドラッグするか [Project の選択を追加] を押すとオンになります。"
                }
            },
            {
                "Package.ExportOffNoTargetsEmbedded.tip",
                new[]
                {
                    "내보낼 프리팹이 없어 꺼져 있습니다. 위 목록에서 프리팹을 체크하면 켜집니다.",
                    "Off because no prefab is selected. Check prefabs in the list above to turn it on.",
                    "書き出すプレハブがないためオフです。上の一覧でプレハブにチェックを入れるとオンになります。"
                }
            },
            {
                "Package.ExportOffNoAssets.tip",
                new[]
                {
                    "포함된 에셋이 없어 꺼져 있습니다. 대상 프리팹이 아직 있는지 확인한 뒤 [새로고침]을 누르면 다시 모읍니다.",
                    "Off because no asset is included. Make sure the target prefabs still exist, then press [Refresh] to gather again.",
                    "含まれるアセットがないためオフです。対象プレハブがまだあるか確認してから [更新] を押すと集め直します。"
                }
            },
            {
                "Package.ExportTitle",
                new[]
                {
                    "패키지 내보내는 중",
                    "Exporting packages",
                    "パッケージ書き出し中"
                }
            },
            {
                "Package.ExportProgress",
                new[]
                {
                    "({0}/{1}) {2}",
                    "({0}/{1}) {2}",
                    "({0}/{1}) {2}"
                }
            },
            {
                "Package.ExportNoTargets",
                new[]
                {
                    "내보낼 프리팹이 없습니다.",
                    "There is no prefab to export.",
                    "書き出すプレハブがありません。"
                }
            },
            {
                "Package.ExportNoAssets",
                new[]
                {
                    "포함된 에셋이 없습니다(전부 제외됨)",
                    "no asset left to pack (everything was excluded)",
                    "含まれるアセットがありません（すべて除外されました）"
                }
            },
            {
                "Package.OverwriteTitle",
                new[]
                {
                    "같은 이름의 파일이 있습니다",
                    "Files already exist",
                    "同じ名前のファイルがあります"
                }
            },
            {
                "Package.OverwriteMessage",
                new[]
                {
                    "저장 폴더에 같은 이름의 파일 {0}개가 이미 있습니다:\n\n{1}\n\n어떻게 할까요?",
                    "{0} file(s) with the same name already exist in the output folder:\n\n{1}\n\nWhat should I do?",
                    "保存フォルダに同じ名前のファイルが {0}件あります:\n\n{1}\n\nどうしますか？"
                }
            },
            {
                "Package.OverwriteMore",
                new[]
                {
                    "… 외 {0}개",
                    "… and {0} more",
                    "… ほか {0}件"
                }
            },
            {
                "Package.OverwriteYes",
                new[]
                {
                    "덮어쓰기",
                    "Overwrite",
                    "上書き"
                }
            },
            {
                "Package.OverwriteRename",
                new[]
                {
                    "_2 붙여 새로 만들기",
                    "Keep both (append _2)",
                    "_2 を付けて新規作成"
                }
            },
            {
                "Package.ResultSkipped",
                new[]
                {
                    "일부 에셋 {0}개는 읽지 못해 패키지에서 빠졌습니다. 콘솔을 확인하세요.",
                    "{0} asset(s) could not be read and are missing from the package. Check the Console.",
                    "一部のアセット {0} 件は読み込めずパッケージに含まれていません。コンソールを確認してください。"
                }
            },
            {
                "Package.ResultOk",
                new[]
                {
                    "완료: 성공 {0}개 · 실패 {1}개 · 총 {2}",
                    "Done: {0} succeeded · {1} failed · {2} total",
                    "完了: 成功 {0}件 · 失敗 {1}件 · 合計 {2}"
                }
            },
            {
                "Package.ResultCanceled",
                new[]
                {
                    "취소됨: {1}개 중 {0}개까지 만들었습니다 (총 {2}). 이미 만들어진 파일은 그대로 남아 있습니다.",
                    "Canceled: {0} of {1} were written ({2} total). The finished files are kept.",
                    "中止: {1}件中 {0}件まで作成しました（合計 {2}）。作成済みのファイルはそのまま残ります。"
                }
            },
            {
                "Package.ResultFailedItem",
                new[]
                {
                    "{0}: {1}",
                    "{0}: {1}",
                    "{0}: {1}"
                }
            },
        };
    }
}
