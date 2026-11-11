package ru.tablo.operator;

import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.InterfaceAddress;
import java.net.NetworkInterface;
import java.net.SocketTimeoutException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Set;

/**
 * Автопоиск табло в локальной сети: UDP-broadcast на порт 8643.
 * Программа «Табло» отвечает строкой TABLO_SERVER_V1|имя|порт|https.
 * Сервер и телефон должны быть в одной подсети; ручной ввод IP остаётся.
 */
public final class LanDiscovery {

    public static final int UDP_PORT = 8643;
    private static final String REQUEST = "TABLO_DISCOVER_V1";
    private static final String ANSWER = "TABLO_SERVER_V1|";

    public static final class Server {
        public final String name;
        public final String url;

        public Server(String name, String url) {
            this.name = name;
            this.url = url;
        }
    }

    private LanDiscovery() {
    }

    public static List<Server> discover(int timeoutMs) {
        List<Server> out = new ArrayList<Server>();
        DatagramSocket sock = null;
        try {
            sock = new DatagramSocket();
            sock.setBroadcast(true);
            sock.setSoTimeout(250);

            byte[] req = REQUEST.getBytes("UTF-8");
            Set<InetAddress> targets = new LinkedHashSet<InetAddress>();
            targets.add(InetAddress.getByName("255.255.255.255"));
            List<NetworkInterface> nis = Collections.list(NetworkInterface.getNetworkInterfaces());
            for (int i = 0; i < nis.size(); i++) {
                NetworkInterface ni = nis.get(i);
                try {
                    if (!ni.isUp() || ni.isLoopback()) {
                        continue;
                    }
                    List<InterfaceAddress> ias = ni.getInterfaceAddresses();
                    for (int j = 0; j < ias.size(); j++) {
                        InetAddress b = ias.get(j).getBroadcast();
                        if (b != null) {
                            targets.add(b);
                        }
                    }
                } catch (Exception ignored) {
                }
            }
            for (InetAddress t : targets) {
                try {
                    sock.send(new DatagramPacket(req, req.length, t, UDP_PORT));
                } catch (Exception ignored) {
                }
            }

            long end = System.currentTimeMillis() + Math.max(500, timeoutMs);
            byte[] buf = new byte[512];
            while (System.currentTimeMillis() < end) {
                DatagramPacket p = new DatagramPacket(buf, buf.length);
                try {
                    sock.receive(p);
                } catch (SocketTimeoutException te) {
                    continue;
                } catch (Exception e) {
                    break;
                }
                String s = new String(p.getData(), 0, p.getLength(), "UTF-8").trim();
                if (!s.startsWith(ANSWER)) {
                    continue;
                }
                String[] a = s.split("\\|");
                if (a.length < 4) {
                    continue;
                }
                String name = a[1];
                String port = a[2];
                boolean https = "1".equals(a[3]);
                String ip = p.getAddress().getHostAddress();
                if (ip != null && ip.indexOf(':') >= 0) {
                    ip = "[" + ip + "]";
                }
                String url = (https ? "https" : "http") + "://" + ip + ":" + port;
                boolean dup = false;
                for (int i = 0; i < out.size(); i++) {
                    if (out.get(i).url.equals(url)) {
                        dup = true;
                        break;
                    }
                }
                if (!dup) {
                    out.add(new Server(name, url));
                }
            }
        } catch (Exception ignored) {
        } finally {
            if (sock != null) {
                try {
                    sock.close();
                } catch (Exception ignored) {
                }
            }
        }
        return out;
    }
}
