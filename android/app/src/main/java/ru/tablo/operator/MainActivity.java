package ru.tablo.operator;

import android.annotation.SuppressLint;
import android.app.ActionBar;
import android.app.Activity;
import android.app.AlertDialog;
import android.app.ProgressDialog;
import android.content.DialogInterface;
import android.content.pm.ActivityInfo;
import android.content.pm.PackageInfo;
import android.graphics.Color;
import android.os.Bundle;
import android.text.InputType;
import android.view.Menu;
import android.view.MenuItem;
import android.view.ScaleGestureDetector;
import android.view.View;
import android.view.WindowManager;
import android.webkit.CookieManager;
import android.webkit.JavascriptInterface;
import android.webkit.RenderProcessGoneDetail;
import android.webkit.WebChromeClient;
import android.webkit.WebResourceError;
import android.webkit.WebResourceRequest;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.PopupMenu;
import android.widget.ScrollView;
import android.widget.TextView;
import android.widget.Toast;

import java.util.List;

/**
 * «Табло — пульт» 1.1 (2026-11). Автор: InJeector.
 *
 * Оболочка веб-панели программы «Табло» (WebView). Работает на Android 8.0+.
 * Меню — компактная кнопка «⋮» поверх панели: поворот, вид управления, размер
 * интерфейса, полный экран, пользовательские кнопки, выход.
 */
public class MainActivity extends Activity {

    private static final int MENU_GROUP_CUSTOM = 10;
    private static final int ID_CUSTOM_BASE = 2000;

    private WebView web;
    private View errorBox;
    private TextView errorDetail;
    private Button btnMenu;
    private boolean loadFailed = false;
    private ScaleGestureDetector scaleDetector;
    private List<Prefs.CustomButton> customButtons;

    @SuppressLint("SetJavaScriptEnabled")
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        applyOrientation();
        setContentView(R.layout.activity_main);

        web = (WebView) findViewById(R.id.web);
        errorBox = findViewById(R.id.errorBox);
        errorDetail = (TextView) findViewById(R.id.errorDetail);
        btnMenu = (Button) findViewById(R.id.btnMenu);

        findViewById(R.id.btnRetry).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                loadPanel();
            }
        });
        findViewById(R.id.btnServer).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                showServerDialog();
            }
        });
        btnMenu.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                showMainMenu();
            }
        });

        WebSettings s = web.getSettings();
        s.setJavaScriptEnabled(true);
        s.setDomStorageEnabled(true);
        s.setSupportZoom(false);
        s.setBuiltInZoomControls(false);
        s.setUseWideViewPort(false);
        s.setLoadWithOverviewMode(false);
        s.setCacheMode(WebSettings.LOAD_NO_CACHE);
        s.setTextZoom(100);
        web.setImportantForAutofill(View.IMPORTANT_FOR_AUTOFILL_YES);
        // Нужен для JS confirm/prompt (предложение сохранить пароль) в WebView.
        web.setWebChromeClient(new WebChromeClient());
        web.addJavascriptInterface(new AndroidBridge(), "TabloAndroid");

        CookieManager.getInstance().setAcceptCookie(true);
        s.setUserAgentString(s.getUserAgentString() + " TabloAndroid/1.2 InJeector");

        web.setBackgroundColor(Color.parseColor("#101010"));
        web.setWebViewClient(new WebViewClient() {
            @Override
            public void onReceivedError(WebView view, WebResourceRequest request, WebResourceError error) {
                super.onReceivedError(view, request, error);
                if (request.isForMainFrame()) {
                    loadFailed = true;
                    CharSequence d = (error != null) ? error.getDescription() : null;
                    showError(d != null ? d.toString() : getString(R.string.error_unknown));
                }
            }

            @Override
            public boolean onRenderProcessGone(WebView view, RenderProcessGoneDetail detail) {
                // 1.2: WebView renderer crashed (e.g. a heavy modal) — recreate instead of freezing.
                try {
                    if (web != null) {
                        web.loadUrl("about:blank");
                        web.destroy();
                    }
                } catch (Exception ignored) {
                }
                web = null;
                recreate();
                return true;
            }

            @Override
            public void onPageFinished(WebView view, String url) {
                super.onPageFinished(view, url);
                applyZoomToPage();
                if (loadFailed) {
                    loadFailed = false;
                } else {
                    errorBox.setVisibility(View.GONE);
                }
            }
        });

        // Pinch: уменьшение — скрыть верхнюю плашку, увеличение — показать.
        scaleDetector = new ScaleGestureDetector(this, new ScaleGestureDetector.SimpleOnScaleGestureListener() {
            @Override
            public boolean onScale(ScaleGestureDetector detector) {
                float f = detector.getScaleFactor();
                if (f < 0.92f && Prefs.showTitle(MainActivity.this)) {
                    setTitleVisible(false);
                } else if (f > 1.08f && !Prefs.showTitle(MainActivity.this)) {
                    setTitleVisible(true);
                }
                return true;
            }
        });
        web.setOnTouchListener(new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, android.view.MotionEvent event) {
                if (scaleDetector != null) {
                    scaleDetector.onTouchEvent(event);
                }
                return false;
            }
        });

        applyTitleBar();
        applySystemUi();
        applyKeepScreenOn();
        loadPanel();
    }

    /** Мост для страниц панели: «Особенности программы» из окна авторизации. */
    public class AndroidBridge {
        @JavascriptInterface
        public void openFeatures() {
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    showFeaturesDialog();
                }
            });
        }

        @JavascriptInterface
        public void setTitleBar(final boolean visible) {
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    setTitleVisible(visible);
                }
            });
        }
    }

    // ---------------- ориентация ----------------

    private void applyOrientation() {
        String o = Prefs.orientation(this);
        if (Prefs.ORIENT_ALBUM.equals(o)) {
            setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_SENSOR_LANDSCAPE);
        } else if (Prefs.ORIENT_HORIZONTAL.equals(o)) {
            setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_LANDSCAPE);
        } else {
            setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_UNSPECIFIED);
        }
    }

    private void setRotation(String mode, int titleRes) {
        Prefs.setOrientation(this, mode);
        applyOrientation();
        Toast.makeText(this, getString(titleRes), Toast.LENGTH_SHORT).show();
    }

    // ---------------- верхняя плашка ----------------

    private void applyTitleBar() {
        ActionBar ab = getActionBar();
        if (ab == null) {
            return;
        }
        try {
            if (!Prefs.fullscreen(this) && Prefs.showTitle(this)) {
                setTitle(R.string.app_name);
                ab.show();
            } else {
                ab.hide();
            }
        } catch (Exception ignored) {
        }
    }

    private void setTitleVisible(boolean visible) {
        Prefs.setShowTitle(this, visible);
        applyTitleBar();
    }

    // ---------------- полный экран / экран ----------------

    private void applySystemUi() {
        View d = getWindow().getDecorView();
        if (Prefs.fullscreen(this)) {
            d.setSystemUiVisibility(
                    View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                            | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
                            | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                            | View.SYSTEM_UI_FLAG_FULLSCREEN
                            | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
                            | View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY);
        } else {
            d.setSystemUiVisibility(0);
        }
        applyTitleBar();
    }

    private void setFullscreen(boolean on) {
        Prefs.setFullscreen(this, on);
        applySystemUi();
    }

    @Override
    protected void onResume() {
        super.onResume();
        applyTitleBar();
        applySystemUi();
        applyKeepScreenOn();
    }

    @Override
    public void onWindowFocusChanged(boolean hasFocus) {
        super.onWindowFocusChanged(hasFocus);
        if (hasFocus && Prefs.fullscreen(this)) {
            applySystemUi();
        }
    }

    private void applyKeepScreenOn() {
        if (Prefs.keepScreenOn(this)) {
            getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        } else {
            getWindow().clearFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        }
    }

    // ---------------- размер интерфейса ----------------

    private void applyZoomToPage() {
        final int pct = Prefs.textZoom(this);
        // Do not set CSS zoom at 100%: avoids a WebView compositor glitch with fixed modals.
        final String js = (pct == 100)
                ? "(function(){try{document.documentElement.style.zoom='';}catch(e){}})();"
                : "(function(){try{document.documentElement.style.zoom='" + (pct / 100.0) + "';}catch(e){}})();";
        try {
            if (web != null) {
                web.evaluateJavascript(js, null);
            }
        } catch (Exception ignored) {
        }
    }

    private void setTextZoom(int pct) {
        if (pct < 50) {
            pct = 50;
        }
        if (pct > 300) {
            pct = 300;
        }
        Prefs.setTextZoom(this, pct);
        applyZoomToPage();
        Toast.makeText(this, getString(R.string.toast_zoom_fmt, pct), Toast.LENGTH_SHORT).show();
    }

    private void showSizeDialog() {
        final int[] vals = new int[] { 75, 100, 125, 150, 200 };
        final String[] names = new String[] { "75%", "100%", "125%", "150%", "200%", getString(R.string.size_custom) };
        int cur = Prefs.textZoom(this);
        int checked = -1;
        for (int i = 0; i < vals.length; i++) {
            if (vals[i] == cur) {
                checked = i;
            }
        }
        new AlertDialog.Builder(this)
                .setTitle(R.string.menu_size)
                .setSingleChoiceItems(names, checked, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface d, int which) {
                        d.dismiss();
                        if (which < vals.length) {
                            setTextZoom(vals[which]);
                        } else {
                            showCustomZoomDialog();
                        }
                    }
                })
                .setNegativeButton(android.R.string.cancel, null)
                .show();
    }

    private void showCustomZoomDialog() {
        final EditText input = new EditText(this);
        input.setInputType(InputType.TYPE_CLASS_NUMBER);
        input.setText(String.valueOf(Prefs.textZoom(this)));
        input.setSelectAllOnFocus(true);
        input.setHint(R.string.size_hint);
        final AlertDialog dlg = new AlertDialog.Builder(this)
                .setTitle(R.string.size_custom_title)
                .setView(input)
                .setPositiveButton(android.R.string.ok, null)
                .setNegativeButton(android.R.string.cancel, null)
                .create();
        dlg.setOnShowListener(new DialogInterface.OnShowListener() {
            @Override
            public void onShow(DialogInterface d) {
                dlg.getButton(AlertDialog.BUTTON_POSITIVE).setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View v) {
                        int v2;
                        try {
                            v2 = Integer.parseInt(input.getText().toString().trim());
                        } catch (Exception e) {
                            input.setError(getString(R.string.size_hint));
                            return;
                        }
                        if (v2 < 50 || v2 > 300) {
                            input.setError(getString(R.string.size_hint));
                            return;
                        }
                        dlg.dismiss();
                        setTextZoom(v2);
                    }
                });
            }
        });
        dlg.show();
    }

    // ---------------- меню ----------------

    private void showMainMenu() {
        PopupMenu popup = new PopupMenu(this, btnMenu);
        popup.getMenuInflater().inflate(R.menu.main_menu, popup.getMenu());
        Menu m = popup.getMenu();

        MenuItem fs = m.findItem(R.id.action_fullscreen);
        fs.setTitle(Prefs.fullscreen(this) ? R.string.menu_fullscreen_off : R.string.menu_fullscreen);
        MenuItem tb = m.findItem(R.id.action_topbar);
        tb.setTitle(Prefs.showTitle(this) ? R.string.menu_topbar_hide : R.string.menu_topbar_show);
        MenuItem ks = m.findItem(R.id.action_keep_screen);
        ks.setTitle((Prefs.keepScreenOn(this) ? "✓ " : "") + getString(R.string.menu_keep_screen));
        String o = Prefs.orientation(this);
        m.findItem(R.id.action_rot_album).setTitle(
                (Prefs.ORIENT_ALBUM.equals(o) ? "✓ " : "") + getString(R.string.menu_rot_album));
        m.findItem(R.id.action_rot_horizontal).setTitle(
                (Prefs.ORIENT_HORIZONTAL.equals(o) ? "✓ " : "") + getString(R.string.menu_rot_horizontal));

        customButtons = Prefs.customButtons(this);
        for (int i = 0; i < customButtons.size(); i++) {
            m.add(MENU_GROUP_CUSTOM, ID_CUSTOM_BASE + i, 200 + i, "▶ " + customButtons.get(i).name);
        }

        popup.setOnMenuItemClickListener(new PopupMenu.OnMenuItemClickListener() {
            @Override
            public boolean onMenuItemClick(MenuItem item) {
                return handleMenu(item);
            }
        });
        popup.show();
    }

    private boolean handleMenu(MenuItem item) {
        int id = item.getItemId();
        if (id == R.id.action_server) {
            showServerDialog();
            return true;
        } else if (id == R.id.action_view) {
            showViewDialog();
            return true;
        } else if (id == R.id.action_size) {
            showSizeDialog();
            return true;
        } else if (id == R.id.action_keep_screen) {
            boolean v = !Prefs.keepScreenOn(this);
            Prefs.setKeepScreenOn(this, v);
            applyKeepScreenOn();
            return true;
        } else if (id == R.id.action_rot_album) {
            setRotation(Prefs.ORIENT_ALBUM, R.string.menu_rot_album);
            return true;
        } else if (id == R.id.action_rot_horizontal) {
            setRotation(Prefs.ORIENT_HORIZONTAL, R.string.menu_rot_horizontal);
            return true;
        } else if (id == R.id.action_fullscreen) {
            setFullscreen(!Prefs.fullscreen(this));
            return true;
        } else if (id == R.id.action_topbar) {
            setTitleVisible(!Prefs.showTitle(this));
            return true;
        } else if (id == R.id.action_features) {
            showFeaturesDialog();
            return true;
        } else if (id == R.id.action_custom_add) {
            showAddCustomDialog();
            return true;
        } else if (id == R.id.action_custom_del) {
            showDeleteCustomDialog();
            return true;
        } else if (id == R.id.action_reload) {
            loadPanel();
            return true;
        } else if (id == R.id.action_about) {
            showAboutDialog();
            return true;
        } else if (id == R.id.action_exit) {
            finishAffinity();
            return true;
        }
        if (id >= ID_CUSTOM_BASE && id < ID_CUSTOM_BASE + 1000) {
            int idx = id - ID_CUSTOM_BASE;
            List<Prefs.CustomButton> list = Prefs.customButtons(this);
            if (idx >= 0 && idx < list.size()) {
                web.loadUrl(resolveUrl(list.get(idx).url));
            }
            return true;
        }
        return false;
    }

    private String resolveUrl(String url) {
        String u = url == null ? "" : url.trim();
        if (u.startsWith("http://") || u.startsWith("https://")) {
            return u;
        }
        String base = Prefs.serverUrl(this).trim();
        while (base.endsWith("/")) {
            base = base.substring(0, base.length() - 1);
        }
        if (!u.startsWith("/")) {
            u = "/" + u;
        }
        return base + u;
    }

    private void showViewDialog() {
        final String cur = Prefs.startPage(this);
        int idx = Prefs.PAGES.indexOf(cur);
        if (idx < 0) {
            idx = 0;
        }
        new AlertDialog.Builder(this)
                .setTitle(R.string.menu_view)
                .setSingleChoiceItems(R.array.view_names, idx, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        Prefs.setStartPage(MainActivity.this, Prefs.PAGES.get(which));
                        dialog.dismiss();
                        loadPanel();
                    }
                })
                .setNegativeButton(android.R.string.cancel, null)
                .show();
    }

    private void showFeaturesDialog() {
        ScrollView scroll = new ScrollView(this);
        LinearLayout box = new LinearLayout(this);
        box.setOrientation(LinearLayout.VERTICAL);
        int pad = (int) (16 * getResources().getDisplayMetrics().density);
        box.setPadding(pad, pad, pad, pad);

        TextView text = new TextView(this);
        text.setText(R.string.features_text);
        text.setTextSize(14);
        text.setLineSpacing(0f, 1.15f);
        box.addView(text);

        final CheckBox cb = new CheckBox(this);
        cb.setText(R.string.features_show_title);
        cb.setChecked(Prefs.showTitle(this));
        LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);
        lp.topMargin = pad;
        box.addView(cb, lp);

        scroll.addView(box);
        AlertDialog dlg = new AlertDialog.Builder(this)
                .setTitle(R.string.menu_features)
                .setView(scroll)
                .setPositiveButton(android.R.string.ok, null)
                .create();
        dlg.setOnDismissListener(new DialogInterface.OnDismissListener() {
            @Override
            public void onDismiss(DialogInterface d) {
                setTitleVisible(cb.isChecked());
            }
        });
        dlg.show();
    }

    // ---------------- пользовательские кнопки ----------------

    private void showAddCustomDialog() {
        LinearLayout box = new LinearLayout(this);
        box.setOrientation(LinearLayout.VERTICAL);
        int pad = (int) (16 * getResources().getDisplayMetrics().density);
        box.setPadding(pad, pad, pad, pad);
        final EditText name = new EditText(this);
        name.setHint(R.string.custom_name_hint);
        final EditText url = new EditText(this);
        url.setHint(R.string.custom_url_hint);
        url.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_URI);
        box.addView(name);
        box.addView(url);

        final AlertDialog dlg = new AlertDialog.Builder(this)
                .setTitle(R.string.menu_custom_add)
                .setView(box)
                .setPositiveButton(android.R.string.ok, null)
                .setNegativeButton(android.R.string.cancel, null)
                .create();
        dlg.setOnShowListener(new DialogInterface.OnShowListener() {
            @Override
            public void onShow(DialogInterface d) {
                dlg.getButton(AlertDialog.BUTTON_POSITIVE).setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View v) {
                        String n = name.getText().toString().trim();
                        String u = url.getText().toString().trim();
                        if (n.length() == 0) {
                            name.setError(getString(R.string.custom_name_hint));
                            return;
                        }
                        if (u.length() == 0) {
                            u = "/operator";
                        }
                        Prefs.addCustomButton(MainActivity.this, n, u);
                        dlg.dismiss();
                        Toast.makeText(MainActivity.this, getString(R.string.toast_custom_added), Toast.LENGTH_SHORT).show();
                    }
                });
            }
        });
        dlg.show();
    }

    private void showDeleteCustomDialog() {
        final List<Prefs.CustomButton> list = Prefs.customButtons(this);
        if (list.isEmpty()) {
            Toast.makeText(this, getString(R.string.custom_empty), Toast.LENGTH_SHORT).show();
            return;
        }
        String[] names = new String[list.size()];
        for (int i = 0; i < list.size(); i++) {
            names[i] = list.get(i).name;
        }
        new AlertDialog.Builder(this)
                .setTitle(R.string.menu_custom_del)
                .setItems(names, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface d, int which) {
                        Prefs.removeCustomButton(MainActivity.this, which);
                        Toast.makeText(MainActivity.this, getString(R.string.toast_custom_deleted), Toast.LENGTH_SHORT).show();
                    }
                })
                .setNegativeButton(android.R.string.cancel, null)
                .show();
    }

    // ---------------- сервер / автопоиск ----------------

    private void loadPanel() {
        loadFailed = false;
        errorBox.setVisibility(View.GONE);
        String base = Prefs.serverUrl(this).trim();
        while (base.endsWith("/")) {
            base = base.substring(0, base.length() - 1);
        }
        web.loadUrl(base + Prefs.startPage(this));
    }

    private void showError(String reason) {
        errorDetail.setText(getString(R.string.error_detail_fmt, Prefs.serverUrl(this), reason));
        errorBox.setVisibility(View.VISIBLE);
    }

    private void showServerDialog() {
        final EditText input = new EditText(this);
        input.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_URI);
        input.setText(Prefs.serverUrl(this));
        input.setHint(R.string.server_hint);
        input.setSelectAllOnFocus(true);
        new AlertDialog.Builder(this)
                .setTitle(R.string.menu_server)
                .setMessage(R.string.server_dialog_msg)
                .setView(input)
                .setPositiveButton(android.R.string.ok, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        String t = input.getText().toString().trim();
                        if (t.length() > 0) {
                            if (!t.startsWith("http://") && !t.startsWith("https://")) {
                                t = "http://" + t;
                            }
                            while (t.endsWith("/")) {
                                t = t.substring(0, t.length() - 1);
                            }
                            Prefs.setServerUrl(MainActivity.this, t);
                            loadPanel();
                        }
                    }
                })
                .setNeutralButton(R.string.server_find, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        discoverServers();
                    }
                })
                .setNegativeButton(android.R.string.cancel, null)
                .show();
    }

    private void discoverServers() {
        final ProgressDialog pd = new ProgressDialog(this);
        pd.setMessage(getString(R.string.server_searching));
        pd.setCancelable(false);
        pd.show();
        new Thread(new Runnable() {
            @Override
            public void run() {
                final List<LanDiscovery.Server> found = LanDiscovery.discover(2600);
                runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        try {
                            pd.dismiss();
                        } catch (Exception ignored) {
                        }
                        showFoundServers(found);
                    }
                });
            }
        }).start();
    }

    private void showFoundServers(final List<LanDiscovery.Server> found) {
        if (found.isEmpty()) {
            new AlertDialog.Builder(this)
                    .setTitle(R.string.servers_found)
                    .setMessage(R.string.servers_none)
                    .setPositiveButton(R.string.server_manual, new DialogInterface.OnClickListener() {
                        @Override
                        public void onClick(DialogInterface d, int which) {
                            showServerDialog();
                        }
                    })
                    .setNegativeButton(android.R.string.cancel, null)
                    .show();
            return;
        }
        String[] labels = new String[found.size()];
        for (int i = 0; i < found.size(); i++) {
            labels[i] = found.get(i).name + "\n" + found.get(i).url;
        }
        new AlertDialog.Builder(this)
                .setTitle(R.string.servers_found)
                .setItems(labels, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface d, int which) {
                        Prefs.setServerUrl(MainActivity.this, found.get(which).url);
                        loadPanel();
                    }
                })
                .setNegativeButton(android.R.string.cancel, null)
                .show();
    }

    private void showAboutDialog() {
        String version = "";
        try {
            PackageInfo pi = getPackageManager().getPackageInfo(getPackageName(), 0);
            version = (pi.versionName == null) ? "" : pi.versionName;
        } catch (Exception ignored) {
        }
        String msg = getString(R.string.about_text_fmt, getString(R.string.app_author), version);
        new AlertDialog.Builder(this)
                .setTitle(R.string.menu_about)
                .setMessage(msg)
                .setPositiveButton(android.R.string.ok, null)
                .show();
    }

    // ---------------- системные кнопки ----------------

    @Override
    public void onBackPressed() {
        if (Prefs.fullscreen(this)) {
            setFullscreen(false);
            return;
        }
        if (web != null && web.canGoBack()) {
            web.goBack();
            return;
        }
        finishAffinity();
    }
}
