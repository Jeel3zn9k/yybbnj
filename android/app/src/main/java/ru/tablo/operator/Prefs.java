package ru.tablo.operator;

import android.content.Context;
import android.content.SharedPreferences;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * Настройки приложения: адрес сервера табло, стартовая страница, «экран всегда включён»,
 * ориентация, полный экран, верхняя плашка, размер интерфейса и пользовательские кнопки.
 * Хранятся в SharedPreferences (локально на телефоне, никуда не передаются).
 */
public final class Prefs {
    private static final String FILE = "tablo_prefs";
    private static final String KEY_SERVER = "server_url";
    private static final String KEY_PAGE = "start_page";
    private static final String KEY_SCREEN = "keep_screen_on";
    private static final String KEY_ORIENTATION = "orientation";
    private static final String KEY_FULLSCREEN = "fullscreen";
    private static final String KEY_TITLE = "show_title";
    private static final String KEY_ZOOM = "text_zoom";
    private static final String KEY_CUSTOM = "custom_buttons";

    public static final String DEFAULT_SERVER = "http://192.168.1.50:8642";
    public static final String DEFAULT_PAGE = "/panel";

    public static final String ORIENT_AUTO = "";
    public static final String ORIENT_ALBUM = "sensor_landscape";
    public static final String ORIENT_HORIZONTAL = "landscape";

    /** Доступные виды (пути на веб-сервере табло): пульт оператора, расширенная панель, витрина. */
    public static final ArrayList<String> PAGES = new ArrayList<String>(
            Arrays.asList("/panel", "/operator", "/display"));

    public static final class CustomButton {
        public final String name;
        public final String url;

        public CustomButton(String name, String url) {
            this.name = name;
            this.url = url;
        }
    }

    private Prefs() {
    }

    private static SharedPreferences p(Context ctx) {
        return ctx.getSharedPreferences(FILE, Context.MODE_PRIVATE);
    }

    public static String serverUrl(Context ctx) {
        return p(ctx).getString(KEY_SERVER, DEFAULT_SERVER);
    }

    public static void setServerUrl(Context ctx, String v) {
        p(ctx).edit().putString(KEY_SERVER, v).apply();
    }

    public static String startPage(Context ctx) {
        return p(ctx).getString(KEY_PAGE, DEFAULT_PAGE);
    }

    public static void setStartPage(Context ctx, String v) {
        p(ctx).edit().putString(KEY_PAGE, v).apply();
    }

    public static boolean keepScreenOn(Context ctx) {
        return p(ctx).getBoolean(KEY_SCREEN, true);
    }

    public static void setKeepScreenOn(Context ctx, boolean v) {
        p(ctx).edit().putBoolean(KEY_SCREEN, v).apply();
    }

    public static String orientation(Context ctx) {
        return p(ctx).getString(KEY_ORIENTATION, ORIENT_AUTO);
    }

    public static void setOrientation(Context ctx, String v) {
        p(ctx).edit().putString(KEY_ORIENTATION, v).apply();
    }

    public static boolean fullscreen(Context ctx) {
        return p(ctx).getBoolean(KEY_FULLSCREEN, false);
    }

    public static void setFullscreen(Context ctx, boolean v) {
        p(ctx).edit().putBoolean(KEY_FULLSCREEN, v).apply();
    }

    /** Верхняя плашка «Табло — пульт»: по умолчанию скрыта (показ — pinch наружу). */
    public static boolean showTitle(Context ctx) {
        return p(ctx).getBoolean(KEY_TITLE, false);
    }

    public static void setShowTitle(Context ctx, boolean v) {
        p(ctx).edit().putBoolean(KEY_TITLE, v).apply();
    }

    public static int textZoom(Context ctx) {
        return p(ctx).getInt(KEY_ZOOM, 100);
    }

    public static void setTextZoom(Context ctx, int v) {
        p(ctx).edit().putInt(KEY_ZOOM, v).apply();
    }

    public static List<CustomButton> customButtons(Context ctx) {
        List<CustomButton> out = new ArrayList<CustomButton>();
        String raw = p(ctx).getString(KEY_CUSTOM, "[]");
        try {
            JSONArray a = new JSONArray(raw);
            for (int i = 0; i < a.length(); i++) {
                JSONObject o = a.optJSONObject(i);
                if (o == null) {
                    continue;
                }
                String name = o.optString("name", "").trim();
                String url = o.optString("url", "").trim();
                if (name.length() > 0 && url.length() > 0) {
                    out.add(new CustomButton(name, url));
                }
            }
        } catch (JSONException ignored) {
        }
        return out;
    }

    public static void saveCustomButtons(Context ctx, List<CustomButton> list) {
        JSONArray a = new JSONArray();
        for (int i = 0; i < list.size(); i++) {
            JSONObject o = new JSONObject();
            try {
                o.put("name", list.get(i).name);
                o.put("url", list.get(i).url);
                a.put(o);
            } catch (JSONException ignored) {
            }
        }
        p(ctx).edit().putString(KEY_CUSTOM, a.toString()).apply();
    }

    public static void addCustomButton(Context ctx, String name, String url) {
        List<CustomButton> list = customButtons(ctx);
        list.add(new CustomButton(name, url));
        saveCustomButtons(ctx, list);
    }

    public static void removeCustomButton(Context ctx, int index) {
        List<CustomButton> list = customButtons(ctx);
        if (index >= 0 && index < list.size()) {
            list.remove(index);
            saveCustomButtons(ctx, list);
        }
    }
}
