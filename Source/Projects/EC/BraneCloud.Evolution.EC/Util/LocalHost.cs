/*
 * BraneCloud.Evolution.EC (Evolutionary Computation)
 * Copyright 2011 Bennett R. Stabile (BraneCloud.Evolution.net|com)
 * Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
 *
 * This is an independent conversion from Java to .NET of ...
 *
 * Sean Luke's ECJ project at GMU 
 * (Academic Free License v3.0): 
 * http://www.cs.gmu.edu/~eclab/projects/ecj
 *
 * Radical alteration was required throughout (including structural).
 * The author of ECJ cannot and MUST not be expected to support this fork.
 *
 * If you wish to create yet another fork, please use a different root namespace.
 * BraneCloud is a registered domain that will be used for name/schema resolution.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace BraneCloud.Evolution.EC.Util
{
    /*
     * Modified from <a href="http://mail-archives.apache.org/mod_mbox/jakarta-jcs-dev/200803.mbox/%3C1066089445.1204637680849.JavaMail.jira@brutus%3E">
     * apache mail-archives</a>. 
     *
     * This code was published in the public domain, but I believe it was probably intended to be
     * distributed under the Apache license, like the rest of the Jakarta code.  That license is
     * listed at the end of this file.
     *
     */

    //[ECConfiguration("ec.util.LocalHost")]
    //public class LocalHost
    //{
    // *
    // * Returns an <code>InetAddress</code> object encapsulating what is most
    // * likely the machine's LAN IP address. <p/> This method is intended for use
    // * as a replacement of JDK method <code>InetAddress.getLocalHost</code>,
    // * because that method is ambiguous on Linux systems. Linux systems
    // * enumerate the loopback network interface the same way as regular LAN
    // * network interfaces, but the JDK <code>InetAddress.getLocalHost</code>
    // * method does not specify the algorithm used to select the address returned
    // * under such circumstances, and will often return the loopback address,
    // * which is not valid for network communication. Details <a
    // * href="http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=4665037"
    // * >here</a>. <p/> This method will scan all IP addresses on all network
    // * interfaces on the host machine to determine the IP address most likely to
    // * be the machine's LAN address. If the machine has multiple IP addresses,
    // * this method will prefer a site-local IP address (e.g. 192.168.x.x or
    // * 10.10.x.x, usually IPv4) if the machine has one (and will return the
    // * first site-local address if the machine has more than one), but if the
    // * machine does not hold a site-local address, this method will return
    // * simply the first non-loopback address found (IPv4 or IPv6). <p/> If this
    // * method cannot find a non-loopback address using this selection algorithm,
    // * it will fall back to calling and returning the result of JDK method
    // * <code>InetAddress.getLocalHost</code>. <p/>
    // * 
    // * @throws UnknownHostException
    // *             If the LAN address of the machine cannot be found.
    // */
    //public static InetAddress GetLocalHost() 
    //    {
    //    try 
    //        {
    //        InetAddress candidateAddress = null;
    //        // Iterate all NICs (network interface cards)...
    //        for (Enumeration ifaces = NetworkInterface.getNetworkInterfaces(); ifaces.hasMoreElements();)
    //            {
    //            NetworkInterface iface = (NetworkInterface) ifaces.nextElement();
    //            // Iterate all IP addresses assigned to each card...
    //            for (Enumeration inetAddrs = iface.getInetAddresses(); inetAddrs.hasMoreElements();)
    //                {
    //                InetAddress inetAddr = (InetAddress) inetAddrs.nextElement();
    //                if (!inetAddr.isLoopbackAddress()) 
    //                    {
    //                    if (inetAddr.isSiteLocalAddress()) return inetAddr;  // Found non-loopback site-local address.

    //                    // Found non-loopback address, but not necessarily site-local.
    //                    // Store it as a candidate to be returned if site-local address is not subsequently found...
    //                    // Note that we don't repeatedly assign non-loopback non-site-local addresses as candidates,
    //                    // only the first. For subsequent iterations, candidate will be non-null.
    //                    else if (candidateAddress == null) candidateAddress = inetAddr;
    //                    }
    //                }
    //            }
    //        // We did not find a site-local address, but we found some other non-loopback address.
    //        // Server might have a non-site-local address assigned to its NIC (or it might be running
    //        // IPv6 which deprecates the "site-local" concept).
    //        // Return this non-loopback candidate address...
    //        if (candidateAddress != null) return candidateAddress;

    //        // At this point, we did not find a non-loopback address.
    //        // Fall back to returning whatever InetAddress.getLocalHost() returns...
    //        InetAddress jdkSuppliedAddress = InetAddress.getLocalHost();
    //        if (jdkSuppliedAddress == null)
    //            throw new UnknownHostException("The JDK InetAddress.getLocalHost() method unexpectedly returned null.");
    //        return jdkSuppliedAddress;
    //        }
    //    catch (Exception e) 
    //        {
    //        UnknownHostException unknownHostException = new UnknownHostException("Failed to determine LAN address: " + e);
    //        unknownHostException.initCause(e);
    //        throw unknownHostException;
    //        }
    //    }
    //}
}