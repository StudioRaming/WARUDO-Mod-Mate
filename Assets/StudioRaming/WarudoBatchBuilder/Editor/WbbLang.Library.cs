using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> LibraryTexts = new Dictionary<string, string[]>
        {
            {
                "Library.Paths",
                new[]
                {
                    "폴더 경로 설정",
                    "Folder paths",
                    "フォルダパス設定"
                }
            },
            {
                "Library.Paths.tip",
                new[]
                {
                    "타입별로 Warudo 설치 폴더의 어디를 읽을지 정합니다. 빌드 탭의 출력 경로와 같은 값을 공유합니다.",
                    "Choose which Warudo install folder is read for each type. Shared with the Build tab's output path.",
                    "タイプごとにWarudoインストールフォルダのどこを読むか設定します。ビルドタブの出力先と共有されます。"
                }
            },
            {
                "Library.Browse",
                new[]
                {
                    "…",
                    "…",
                    "…"
                }
            },
            {
                "Library.Browse.tip",
                new[]
                {
                    "폴더 선택 창을 엽니다.",
                    "Open the folder picker.",
                    "フォルダ選択ウィンドウを開きます。"
                }
            },
            {
                "Library.BrowseTitle",
                new[]
                {
                    "{0} 폴더 선택",
                    "Select the {0} folder",
                    "{0} フォルダを選択"
                }
            },
            {
                "Library.SetPath",
                new[]
                {
                    "경로 지정",
                    "Set path",
                    "パスを指定"
                }
            },
            {
                "Library.SetPath.tip",
                new[]
                {
                    "읽을 폴더를 직접 골라 지정합니다.",
                    "Pick the folder to read from.",
                    "読み込むフォルダを直接指定します。"
                }
            },
            {
                "Library.PathEmpty",
                new[]
                {
                    "{0}: 경로가 비어 있습니다.",
                    "{0}: path is empty.",
                    "{0}: パスが未設定です。"
                }
            },
            {
                "Library.FolderMissing",
                new[]
                {
                    "{0}: 폴더를 찾을 수 없습니다 ({1})",
                    "{0}: folder not found ({1})",
                    "{0}: フォルダが見つかりません ({1})"
                }
            },
            {
                "Library.ScanError",
                new[]
                {
                    "{0}: 읽기 실패 ({1})",
                    "{0}: read failed ({1})",
                    "{0}: 読み込み失敗 ({1})"
                }
            },
            {
                "Library.FilterProp.tip",
                new[]
                {
                    "Props 폴더의 산출물을 목록에 표시할지 켜고 끕니다.",
                    "Show or hide outputs from the Props folder.",
                    "Propsフォルダの出力を一覧に表示するか切り替えます。"
                }
            },
            {
                "Library.FilterCharacter.tip",
                new[]
                {
                    "Characters 폴더의 산출물(.warudo / .vrm)을 목록에 표시할지 켜고 끕니다.",
                    "Show or hide outputs (.warudo / .vrm) from the Characters folder.",
                    "Charactersフォルダの出力(.warudo / .vrm)を一覧に表示するか切り替えます。"
                }
            },
            {
                "Library.FilterEnvironment.tip",
                new[]
                {
                    "Environments 폴더의 산출물을 목록에 표시할지 켜고 끕니다.",
                    "Show or hide outputs from the Environments folder.",
                    "Environmentsフォルダの出力を一覧に表示するか切り替えます。"
                }
            },
            {
                "Library.SearchHint",
                new[]
                {
                    "이름 부분일치로 걸러냅니다. 토큰을 조합할 수도 있습니다:\n" + "type:prop  ext:vrm  folder:Acc  author:이름  ver:1.0  state:tracked  state:untracked\n" + "여러 토큰은 공백으로 구분하며 전부 만족하는 항목만 남습니다.",
                    "Filter by partial name match. Tokens can be combined:\n" + "type:prop  ext:vrm  folder:Acc  author:name  ver:1.0  state:tracked  state:untracked\n" + "Space-separated tokens must all match.",
                    "名前の部分一致で絞り込みます。トークンの組み合わせも可能です:\n" + "type:prop  ext:vrm  folder:Acc  author:名前  ver:1.0  state:tracked  state:untracked\n" + "スペース区切りのトークンは全て満たす項目のみ残ります。"
                }
            },
            {
                "Library.SortMode",
                new[]
                {
                    "정렬",
                    "Sort",
                    "並び替え"
                }
            },
            {
                "Library.SortMode.tip",
                new[]
                {
                    "목록 정렬 기준을 바꿉니다.",
                    "Change how the list is ordered.",
                    "一覧の並び順を変更します。"
                }
            },
            {
                "Library.BadgeTracked",
                new[]
                {
                    "추적됨",
                    "Tracked",
                    "追跡中"
                }
            },
            {
                "Library.BadgeTracked.tip",
                new[]
                {
                    "이 프로젝트의 빌드 이력과 연결된 산출물입니다. 소스 프리팹 선택과 재빌드가 가능합니다.",
                    "Linked to a build record in this project. Source prefab and rebuild are available.",
                    "このプロジェクトのビルド履歴と紐づいた出力です。ソースプレハブ選択と再ビルドが可能です。"
                }
            },
            {
                "Library.BadgeUntracked",
                new[]
                {
                    "외부 파일",
                    "External",
                    "外部ファイル"
                }
            },
            {
                "Library.BadgeUntracked.tip",
                new[]
                {
                    "이 프로젝트에서 빌드한 기록이 없는 파일입니다(직접 넣었거나 다른 PC에서 빌드).",
                    "No build record in this project (added manually or built elsewhere).",
                    "このプロジェクトでのビルド記録がないファイルです(手動配置または別PCでビルド)。"
                }
            },
            {
                "Library.BadgeFailed",
                new[]
                {
                    "실패 이력",
                    "Failed record",
                    "失敗履歴"
                }
            },
            {
                "Library.BadgeFailed.tip",
                new[]
                {
                    "같은 이름의 마지막 빌드가 실패로 기록되어 있습니다. 파일은 이전 빌드의 잔여물일 수 있습니다.",
                    "The last build with this name is recorded as failed. This file may be left over from an older build.",
                    "同名の最後のビルドが失敗として記録されています。ファイルは以前のビルドの残骸の可能性があります。"
                }
            },
            {
                "Library.BadgeHistory",
                new[]
                {
                    "이력 있음",
                    "Has record",
                    "履歴あり"
                }
            },
            {
                "Library.BadgeHistory.tip",
                new[]
                {
                    "같은 이름의 빌드 기록이 있지만 성공으로 끝나지 않았습니다(중단/건너뜀).",
                    "A build record exists for this name but it did not finish successfully (canceled or skipped).",
                    "同名のビルド記録はありますが正常終了していません(中断・スキップ)。"
                }
            },
            {
                "Library.NoVersion",
                new[]
                {
                    "—",
                    "—",
                    "—"
                }
            },
            {
                "Library.RowTip",
                new[]
                {
                    "{0}\n{1}\n크기 {2} · 수정 {3}\n{4}",
                    "{0}\n{1}\nSize {2} · Modified {3}\n{4}",
                    "{0}\n{1}\nサイズ {2} · 更新 {3}\n{4}"
                }
            },
            {
                "Library.RowTipTracked",
                new[]
                {
                    "빌드 이력: v{0} · {1} · {2}\n소스 프리팹: {3}",
                    "Build record: v{0} · {1} · {2}\nSource prefab: {3}",
                    "ビルド履歴: v{0} · {1} · {2}\nソースプレハブ: {3}"
                }
            },
            {
                "Library.RowTipUntracked",
                new[]
                {
                    "빌드 이력 없음: 이 프로젝트에서 빌드한 기록을 찾지 못했습니다.",
                    "No build record: this project has no matching build history.",
                    "ビルド履歴なし: このプロジェクトに一致する記録がありません。"
                }
            },
            {
                "Library.RowMenu.tip",
                new[]
                {
                    "이 항목의 동작 메뉴를 엽니다(우클릭과 같습니다).",
                    "Open the action menu for this item (same as right-click).",
                    "この項目の操作メニューを開きます(右クリックと同じ)。"
                }
            },
            {
                "Library.TypeChipProp",
                new[]
                {
                    "소품 {0} · {1}",
                    "Prop {0} · {1}",
                    "小物 {0} · {1}"
                }
            },
            {
                "Library.TypeChipCharacter",
                new[]
                {
                    "캐릭터 {0} · {1}",
                    "Character {0} · {1}",
                    "キャラ {0} · {1}"
                }
            },
            {
                "Library.TypeChipEnvironment",
                new[]
                {
                    "환경 {0} · {1}",
                    "Environment {0} · {1}",
                    "環境 {0} · {1}"
                }
            },
            {
                "Library.Summary",
                new[]
                {
                    "전체 {0}개 · {1} · 추적됨 {2}개 · 미추적 {3}개",
                    "{0} files · {1} · {2} tracked · {3} untracked",
                    "全 {0}件 · {1} · 追跡中 {2}件 · 未追跡 {3}件"
                }
            },
            {
                "Library.FilteredCount",
                new[]
                {
                    "표시 {0}개 / 전체 {1}개",
                    "Showing {0} of {1}",
                    "表示 {0}件 / 全 {1}件"
                }
            },
            {
                "Library.SelectionSummary",
                new[]
                {
                    "선택 {0}개 · 합계 {1}",
                    "{0} selected · {1} total",
                    "選択 {0}件 · 合計 {1}"
                }
            },
            {
                "Library.NoTypeEnabled",
                new[]
                {
                    "표시할 타입이 없습니다. 위의 타입 버튼(소품 · 캐릭터 · 환경) 중 하나 이상을 켜 주세요.",
                    "No type is enabled. Turn on at least one of the type buttons above (Prop / Character / Environment).",
                    "表示するタイプがありません。上のタイプボタン(小物・キャラ・環境)のいずれかをオンにしてください。"
                }
            },
            {
                "Library.EmptyNoPathTitle",
                new[]
                {
                    "모드 폴더가 지정되지 않았습니다",
                    "No mod folder set",
                    "MODフォルダが未設定です"
                }
            },
            {
                "Library.EmptyNoPathBody",
                new[]
                {
                    "WARUDO 설치 폴더 안의 Props · Characters · Environments 폴더를 지정하면 빌드한 모드가 여기에 나옵니다.",
                    "Set the Props, Characters and Environments folders inside your WARUDO install to list your built mods here.",
                    "WARUDOインストールフォルダ内の Props・Characters・Environments フォルダを指定すると、ビルドしたMODがここに表示されます。"
                }
            },
            {
                "Library.EmptyBadPathTitle",
                new[]
                {
                    "폴더를 열 수 없습니다",
                    "Can't open the folder",
                    "フォルダを開けません"
                }
            },
            {
                "Library.EmptyBadPathBody",
                new[]
                {
                    "지정한 폴더가 없거나 읽을 수 없습니다. 경로를 다시 지정하세요.",
                    "The folder is missing or can't be read. Set the path again.",
                    "指定したフォルダがないか、読み込めません。パスを指定し直してください。"
                }
            },
            {
                "Library.EmptyNoFilesTitle",
                new[]
                {
                    "아직 빌드한 모드가 없습니다",
                    "No built mods yet",
                    "ビルドしたMODはまだありません"
                }
            },
            {
                "Library.EmptyNoFilesBody",
                new[]
                {
                    "폴더에 모드 파일이 없습니다. 빌드 탭에서 빌드하면 여기에 나옵니다.",
                    "The folders have no mod files. Build on the Build tab and they show up here.",
                    "フォルダにMODファイルがありません。ビルドタブでビルドするとここに表示されます。"
                }
            },
            {
                "Library.EmptyGoBuild",
                new[]
                {
                    "빌드 탭으로",
                    "Go to Build",
                    "ビルドタブへ"
                }
            },
            {
                "Library.MenuReveal",
                new[]
                {
                    "폴더 열기",
                    "Open Folder",
                    "フォルダを開く"
                }
            },
            {
                "Library.MenuCopyPath",
                new[]
                {
                    "전체 경로 복사",
                    "Copy full path",
                    "フルパスをコピー"
                }
            },
            {
                "Library.MenuPingPrefab",
                new[]
                {
                    "소스 프리팹 선택",
                    "Select source prefab",
                    "ソースプレハブを選択"
                }
            },
            {
                "Library.MenuPingPrefabMissing",
                new[]
                {
                    "소스 프리팹 선택 (이력 없음)",
                    "Select source prefab (no record)",
                    "ソースプレハブを選択 (履歴なし)"
                }
            },
            {
                "Library.MenuRebuildMissing",
                new[]
                {
                    "재빌드 (이력 없음)",
                    "Rebuild (no record)",
                    "再ビルド (履歴なし)"
                }
            },
            {
                "Library.RebuildMixedFolders",
                new[]
                {
                    "선택한 항목의 타입 · 버전 · 제작자 · 출력 폴더가 {1}가지로 나뉩니다. 한 번에 빌드할 수 있는 가장 큰 묶음만 넘깁니다. 남은 {0}개는 이 빌드 뒤에 다시 요청하세요.",
                    "The selected items split into {1} groups by type, version, author or output folder. Only the largest group that can be built together is queued. Request the remaining {0} again after this build.",
                    "選択項目はタイプ・バージョン・作者・出力フォルダで {1} 組に分かれます。一度にビルドできる最も大きな組だけを送ります。残り {0} 件はこのビルドのあとで再度リクエストしてください。"
                }
            },
            {
                "Library.DeleteHiddenWarn",
                new[]
                {
                    "이 중 {0}개는 현재 검색·필터 때문에 화면에 보이지 않는 항목입니다.",
                    "{0} of these are not visible on screen right now because of the current search/filter.",
                    "このうち {0}件は現在の検索・フィルターにより画面に表示されていない項目です。"
                }
            },
            {
                "Library.MenuDeleteOne",
                new[]
                {
                    "이 파일만 삭제  ({0})",
                    "Delete only this file  ({0})",
                    "このファイルのみ削除  ({0})"
                }
            },
            {
                "Library.DeleteOneConfirm",
                new[]
                {
                    "이 파일 1개를 휴지통으로 보냅니다. 휴지통이 받지 않으면 파일 옆 _WBB_Trash 폴더로 옮깁니다. 어느 쪽이든 나중에 되돌릴 수 있습니다.\n\n{0}  ({1})\n\n{2}",
                    "This moves 1 file to the Recycle Bin. If the Recycle Bin refuses it, it is moved to a _WBB_Trash folder next to the file. Either way, you can restore it later.\n\n{0}  ({1})\n\n{2}",
                    "このファイル1件をごみ箱に移動します。ごみ箱に入らない場合は、ファイルと同じ場所の _WBB_Trash フォルダへ移動します。どちらの場合も後で元に戻せます。\n\n{0}  ({1})\n\n{2}"
                }
            },
            {
                "Library.DeleteNukeNote",
                new[]
                {
                    "Windows가 \"영구히 삭제할까요?\"라고 물으면 [아니요]를 누르세요. 그러면 _WBB_Trash 폴더로 옮깁니다.",
                    "If Windows asks whether to delete a file permanently, choose No. It is then moved to the _WBB_Trash folder instead.",
                    "Windows が「完全に削除しますか?」と確認してきたら[いいえ]を選んでください。代わりに _WBB_Trash フォルダへ移動します。"
                }
            },
            {
                "Library.CopiedPath",
                new[]
                {
                    "경로를 클립보드에 복사했습니다: {0}",
                    "Path copied to clipboard: {0}",
                    "パスをクリップボードにコピーしました: {0}"
                }
            },
            {
                "Library.PrefabMissing",
                new[]
                {
                    "소스 프리팹을 찾을 수 없습니다: {0}",
                    "Source prefab not found: {0}",
                    "ソースプレハブが見つかりません: {0}"
                }
            },
            {
                "Library.DeleteSelected",
                new[]
                {
                    "선택 삭제",
                    "Delete selected",
                    "選択を削除"
                }
            },
            {
                "Library.DeleteSelected.tip",
                new[]
                {
                    "선택한 산출물 파일을 휴지통으로 보냅니다. 휴지통이 받지 않는 파일은 파일 옆 _WBB_Trash 폴더로 옮깁니다.",
                    "Move the selected output files to the Recycle Bin. Files the Recycle Bin refuses are moved to a _WBB_Trash folder next to them.",
                    "選択した出力ファイルをごみ箱に移動します。ごみ箱に入らないファイルは、同じ場所の _WBB_Trash フォルダへ移動します。"
                }
            },
            {
                "Library.DeleteSelectedNone.tip",
                new[]
                {
                    "선택한 파일이 없어서 꺼져 있습니다. 목록에서 지울 파일을 체크하면 켜집니다.",
                    "Off because no file is selected. Check the files to delete in the list to turn it on.",
                    "選択したファイルがないためオフです。一覧で削除するファイルにチェックするとオンになります。"
                }
            },
            {
                "Library.DeleteTitle",
                new[]
                {
                    "산출물 삭제",
                    "Delete outputs",
                    "出力の削除"
                }
            },
            {
                "Library.DeleteConfirm",
                new[]
                {
                    "산출물 {0}개(총 {1})를 휴지통으로 보냅니다.\n휴지통이 받지 않는 파일은 파일 옆 _WBB_Trash 폴더로 옮깁니다. 어느 쪽이든 나중에 되돌릴 수 있습니다.\n\n{2}",
                    "{0} output file(s), {1} in total, will be moved to the Recycle Bin.\nFiles the Recycle Bin refuses are moved to a _WBB_Trash folder next to them. Either way, you can restore them later.\n\n{2}",
                    "出力ファイル {0}件(合計 {1})をごみ箱に移動します。\nごみ箱に入らないファイルは、同じ場所の _WBB_Trash フォルダへ移動します。どちらの場合も後で元に戻せます。\n\n{2}"
                }
            },
            {
                "Library.DeleteMore",
                new[]
                {
                    "… 외 {0}개",
                    "… and {0} more",
                    "… 他 {0}件"
                }
            },
            {
                "Library.DeleteProgress",
                new[]
                {
                    "휴지통으로 옮기는 중… ({0}/{1})",
                    "Moving to the Recycle Bin… ({0}/{1})",
                    "ごみ箱へ移動中… ({0}/{1})"
                }
            },
            {
                "Library.DeleteResult",
                new[]
                {
                    "{0}개를 옮겼습니다. 실패 {1}개. (대상 목록은 콘솔 로그에 남겨 두었습니다)",
                    "Moved {0} file(s), {1} failed. (The list was written to the console log.)",
                    "{0}件を移動しました。失敗 {1}件。(対象リストはコンソールログに残しています)"
                }
            },
            {
                "Library.DeleteCanceled",
                new[]
                {
                    "중간에 멈췄습니다. 남은 {0}개는 건드리지 않았습니다.",
                    "Stopped partway. The remaining {0} file(s) were left untouched.",
                    "途中で停止しました。残りの {0}件はそのままです。"
                }
            },
            {
                "Library.DeleteOutcomeCount",
                new[]
                {
                    "{1}개: {0}",
                    "{1} file(s): {0}",
                    "{1}件: {0}"
                }
            },
            {
                "Library.TrashFolderAt",
                new[]
                {
                    "위치: {0}",
                    "Location: {0}",
                    "場所: {0}"
                }
            },
            {
                "Library.TrashRestoreHint",
                new[]
                {
                    "되돌리려면 파일을 원래 폴더로 옮기고 이름 끝의 {0} 를 지우세요.",
                    "To restore, move the file back and remove {0} from the end of its name.",
                    "元に戻すには、ファイルを元のフォルダへ戻し、名前末尾の {0} を削除してください。"
                }
            },
            {
                "Library.DeleteFailedItem",
                new[]
                {
                    "삭제 실패: {0} ({1})",
                    "Delete failed: {0} ({1})",
                    "削除失敗: {0} ({1})"
                }
            },
            {
                "Library.DeleteNone",
                new[]
                {
                    "삭제할 항목이 선택되지 않았습니다. 목록에서 행을 클릭해 선택하세요.",
                    "Nothing is selected. Click a row in the list to select it.",
                    "削除する項目が選択されていません。一覧の行をクリックして選択してください。"
                }
            },
            {
                "Library.RebuildSelected",
                new[]
                {
                    "선택 재빌드",
                    "Rebuild selected",
                    "選択を再ビルド"
                }
            },
            {
                "Library.RebuildSelected.tip",
                new[]
                {
                    "빌드 이력이 연결된 선택 항목을 빌드 탭 대기열에 넣습니다. 이력이 없는 항목은 제외됩니다.",
                    "Queue the selected items that have a build record into the Build tab. Items without a record are skipped.",
                    "ビルド履歴が紐づいた選択項目をビルドタブのキューに入れます。履歴のない項目は除外されます。"
                }
            },
            {
                "Library.RebuildSelectedNone.tip",
                new[]
                {
                    "선택한 파일이 없어서 꺼져 있습니다. 목록에서 '추적됨' 파일을 체크하면 켜집니다.",
                    "Off because no file is selected. Check a 'Tracked' file in the list to turn it on.",
                    "選択したファイルがないためオフです。一覧で「追跡中」のファイルにチェックするとオンになります。"
                }
            },
            {
                "Library.RebuildSelectedUntracked.tip",
                new[]
                {
                    "선택한 파일에 빌드 이력이 없어서 꺼져 있습니다. 재빌드는 '추적됨' 파일만 할 수 있습니다. '추적됨' 파일을 선택하면 켜집니다.",
                    "Off because none of the selected files has a build record. Only 'Tracked' files can be rebuilt; select one to turn it on.",
                    "選択したファイルにビルド履歴がないためオフです。再ビルドできるのは「追跡中」のファイルだけです。選択するとオンになります。"
                }
            },
            {
                "Library.RebuildTitle",
                new[]
                {
                    "재빌드",
                    "Rebuild",
                    "再ビルド"
                }
            },
            {
                "Library.RebuildSend",
                new[]
                {
                    "빌드 탭으로 보내기",
                    "Send to Build tab",
                    "ビルドタブに送る"
                }
            },
            {
                "Library.StatusClose.tip",
                new[]
                {
                    "이 알림을 닫습니다.",
                    "Close this message.",
                    "このお知らせを閉じます。"
                }
            },
            {
                "Library.RebuildConfirm",
                new[]
                {
                    "선택한 {0}개 중 빌드 이력이 있는 {1}개를 빌드 탭 대기열에 넣습니다.\n계속할까요?",
                    "{1} of the {0} selected item(s) have a build record and will be queued in the Build tab.\nContinue?",
                    "選択した {0}件のうち、ビルド履歴のある {1}件をビルドタブのキューに入れます。\n続行しますか?"
                }
            },
            {
                "Guide.Library",
                new[]
                {
                    "<b>1.</b> 위쪽 <b>폴더 경로 설정</b>을 펼쳐 Prop / Character / Environment 세 폴더가 Warudo 설치 폴더를 가리키는지 확인합니다. 여기서 정한 경로는 빌드 탭의 출력 경로와 공유됩니다.\n" + "<b>2.</b> 타입 버튼으로 볼 종류만 켭니다. 버튼마다 그 타입의 개수와 용량이 함께 보이고, 바로 아래 요약 줄에는 전체 개수·용량과 <b>추적됨 / 미추적</b> 개수가 나옵니다.\n" + "<b>3.</b> 검색창에 이름 일부를 넣거나 토큰을 조합합니다: <b>type:prop</b>, <b>ext:vrm</b>, <b>folder:Acc</b>, <b>author:이름</b>, <b>ver:1.0</b>, <b>state:tracked</b>, <b>state:untracked</b>.\n" + "<b>4.</b> 행을 클릭하면 그 항목만 선택됩니다. <b>Ctrl+클릭</b>은 개별 토글, <b>Shift+클릭</b>은 범위 선택, <b>더블클릭</b>은 그 파일이 있는 폴더를 엽니다(<b>폴더 열기</b>). 우클릭 또는 행 오른쪽 <b>메뉴 버튼</b>으로 동작 메뉴가 열립니다.\n" + "<b>5.</b> <b>추적됨</b> 배지가 붙은 항목은 이 프로젝트의 빌드 이력과 연결되어 있어 소스 프리팹 선택과 재빌드가 가능합니다. <b>외부 파일</b>은 이 프로젝트에서 빌드한 기록이 없는 파일입니다.\n" + "<b>6.</b> 테스트 잔여물 정리는 <b>state:untracked</b> 로 걸러 전체 선택한 뒤 <b>선택 삭제</b>가 가장 빠릅니다. 지운 파일은 휴지통으로 가고(휴지통이 받지 않으면 파일 옆 <b>_WBB_Trash</b> 폴더) 나중에 되돌릴 수 있습니다. 확인 창은 개수와 총 용량을 보여 주며 한 번 뜨고, 결과는 아래 상태 줄에, 대상 목록은 콘솔에 남습니다. 선택한 것이 없으면 <b>선택 재빌드 · 선택 삭제</b>는 꺼져 있습니다.",
                    "<b>1.</b> Expand <b>Folder paths</b> at the top and make sure Prop / Character / Environment point at your Warudo install. These paths are shared with the Build tab's output path.\n" + "<b>2.</b> Use the type buttons to choose what to list. Each button shows that type's count and size; the summary line right below shows the overall count and size plus <b>tracked / untracked</b> counts.\n" + "<b>3.</b> Type part of a name in the search box, or combine tokens: <b>type:prop</b>, <b>ext:vrm</b>, <b>folder:Acc</b>, <b>author:name</b>, <b>ver:1.0</b>, <b>state:tracked</b>, <b>state:untracked</b>.\n" + "<b>4.</b> Clicking a row selects only that item. <b>Ctrl+click</b> toggles, <b>Shift+click</b> selects a range, <b>double-click</b> opens the folder holding the file (<b>Open Folder</b>). Right-click or the <b>menu button</b> at the right of the row opens the action menu.\n" + "<b>5.</b> Items badged <b>Tracked</b> are linked to this project's build history, so the source prefab can be selected and the mod rebuilt. <b>External</b> means no matching build record exists here.\n" + "<b>6.</b> To clean up test leftovers, filter with <b>state:untracked</b>, select all, then use <b>Delete selected</b>. Removed files go to the Recycle Bin (or to a <b>_WBB_Trash</b> folder next to the file if the Recycle Bin refuses them), so you can restore them later. One confirmation shows the exact count and total size; the result stays in the status line below and the list is written to the console. With nothing selected, <b>Rebuild selected</b> and <b>Delete selected</b> are off.",
                    "<b>1.</b> 上部の<b>フォルダパス設定</b>を開き、Prop / Character / Environment がWarudoインストール先を指しているか確認します。ここで設定したパスはビルドタブの出力先と共有されます。\n" + "<b>2.</b> タイプボタンで表示する種類を選びます。各ボタンにそのタイプの件数とサイズが表示され、すぐ下の要約行には全体の件数・サイズと<b>追跡中 / 未追跡</b>件数が出ます。\n" + "<b>3.</b> 検索欄に名前の一部を入れるか、トークンを組み合わせます: <b>type:prop</b>、<b>ext:vrm</b>、<b>folder:Acc</b>、<b>author:名前</b>、<b>ver:1.0</b>、<b>state:tracked</b>、<b>state:untracked</b>。\n" + "<b>4.</b> 行をクリックするとその項目だけが選択されます。<b>Ctrl+クリック</b>で個別トグル、<b>Shift+クリック</b>で範囲選択、<b>ダブルクリック</b>でそのファイルがあるフォルダを開きます(<b>フォルダを開く</b>)。右クリックまたは行の右端の<b>メニューボタン</b>で操作メニューが開きます。\n" + "<b>5.</b> <b>追跡中</b>バッジの項目はこのプロジェクトのビルド履歴と紐づいており、ソースプレハブ選択と再ビルドが可能です。<b>外部ファイル</b>はこのプロジェクトでのビルド記録がないファイルです。\n" + "<b>6.</b> テスト残骸の整理は <b>state:untracked</b> で絞って全選択し<b>選択を削除</b>が最速です。削除したファイルはごみ箱に入り(ごみ箱に入らない場合はファイルと同じ場所の <b>_WBB_Trash</b> フォルダ)、後で元に戻せます。件数と合計サイズを示す確認は1回表示され、結果は下の状態行に、対象リストはコンソールに残ります。選択がないときは<b>選択を再ビルド・選択を削除</b>はオフです。"
                }
            },
        };
    }
}
