ePubMaker
=========

はじめに
--------
ePubMakerは自炊データからePubファイルを生成するアプリケーションです。
XP以降のWindows上で動作します(.NET Framework 4が必要です)。

なお、現状、ソース形式でのみ配布されているわけですが、実際に実行ファイルを
生成するにはVisual C# 2010が必要となります(Expressでも可)。


使い方
------
大前提として、スキャンして出来た画像ファイル(PNGやBMPも読み込みますが、
JPEGを想定しています)がどこかのフォルダにベタに格納されていて、ファイル名で
ソートすれば正しくページ順であることを想定しています。

メニューの「ファイル」→「元データフォルダを開く」でそのフォルダを選択すると、
画面左領域のページ一覧にそのフォルダ内の画像ファイル名がずらずらと
読み込まれます。
読み込みが終わったら、まずは一番右の「出力設定」の各項目を埋めておいてください。

その後、ページ一覧で適当にページを選択すると、真ん中左に元画像が、真ん中右に
生成される画像のプレビューが、それぞれ適当な縮尺で表示されます。
それを眺めながら、一番右の「画像設定像」の項目をいじって、回転させてみたり、
画像形式を変えてみたりしてください。
また、ページ一覧の「目次」項目に適当な文字列を入力すると、そのページがその
名前で目次に出力されることになります。

なお、ページ一覧は複数選択が可能なので、一度に複数のページに対して設定を行う
ことができます。
選択を楽にするために、ページ一覧の上に「全ページ選択」「奇数ページ選択」
「偶数ページ選択」のボタンを用意しておいたので、活用してください。
また、ページ一覧の「ロック」をチェックすると、そのページはこれらのボタンを
押しても選択されなくなるので、ほかのページとは違う特別なページがある場合は
これをチェックしておいて個別に設定を行う、ということもできます。

各ページの設定が終わったら、メニューの「ePub」→「生成」を実行すると、
出力先の設定ダイアログが開き、適当な場所・ファイル名を選べばePubファイルの
生成が行われます。
それなりに時間がかかるので、のんびり待ってください。


ライセンス
----------
めんどくさいのでソース中にはまだ書いてないですが、いわゆる
2-clause BSD licenseです。
あんまり詳しくないのですが、バイナリを配布する際は、私の提示しているこの
ライセンス以外に、Microsoftさんの言うことも考慮する必要があるんじゃないかと
思います。

Copyright (c) 2012  NAKAMURA Usaku <usa@garbagecollect.jp>

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR AND CONTRIBUTORS ``AS IS'' AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR OR CONTRIBUTORS
BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN
IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


TODO
----
* ページ分割
* ページ並べ替え
* 濃度調整
* 傾き補正
* 選択ボタンのあたりの見栄えとか
* その他現状手抜きなところ
