/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package org.apache.ignite.cache.affinity.trex;

import org.apache.ignite.IgniteLogger;
import org.apache.ignite.binary.BinaryReader;
import org.apache.ignite.cache.affinity.AffinityFunction;
import org.apache.ignite.cache.affinity.AffinityFunctionContext;
import org.apache.ignite.cluster.ClusterNode;
import org.apache.ignite.configuration.CacheConfiguration;
import org.apache.ignite.internal.binary.BinaryObjectImpl;
import org.apache.ignite.internal.binary.BinaryReaderExImpl;
import org.apache.ignite.internal.binary.streams.BinaryByteBufferInputStream;
import org.apache.ignite.internal.processors.cache.GridCacheUtils;
import org.apache.ignite.internal.util.typedef.F;
import org.apache.ignite.internal.util.typedef.internal.A;
import org.apache.ignite.internal.util.typedef.internal.LT;
import org.apache.ignite.internal.util.typedef.internal.U;
import org.apache.ignite.lang.IgniteBiPredicate;
import org.apache.ignite.lang.IgniteBiTuple;
import org.apache.ignite.resources.LoggerResource;
import org.jetbrains.annotations.Nullable;

import java.io.IOException;
import java.io.Serializable;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.*;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Affinity function for partitioned cache based on Highest Random Weight algorithm.
 * This function supports the following configuration:
 * <ul>
 * <li>
 *      {@code partitions} - Number of partitions to spread across nodes.
 * </li>
 * <li>
 *      {@code excludeNeighbors} - If set to {@code true}, will exclude same-host-neighbors
 *      from being backups of each other. This flag can be ignored in cases when topology has no enough nodes
 *      for assign backups.
 *      Note that {@code backupFilter} is ignored if {@code excludeNeighbors} is set to {@code true}.
 * </li>
 * <li>
 *      {@code backupFilter} - Optional filter for back up nodes. If provided, then only
 *      nodes that pass this filter will be selected as backup nodes. If not provided, then
 *      primary and backup nodes will be selected out of all nodes available for this cache.
 * </li>
 * </ul>
 * <p>
 * Cache affinity can be configured for individual caches via {@link CacheConfiguration#getAffinity()} method.
 */
public class TrexMutableNonSpatialAffinityFunction implements AffinityFunction, Serializable {

    int numPartitions = 1024;

    @Override
    public void reset() {

    }

    @Override
    public int partitions() {
        return numPartitions;
    }

    @Override
    public int partition(Object key) {
        BinaryByteBufferInputStream buff = BinaryByteBufferInputStream.create(ByteBuffer.wrap(((BinaryObjectImpl) key).array()).order(ByteOrder.LITTLE_ENDIAN));
        BinaryReader r = new BinaryReaderExImpl(((BinaryObjectImpl) key).context(),
                buff,
                null, false);

        try {
            ((BinaryReaderExImpl) r).skipBytes(25); //25 = 24 header + 1 version
        } catch (IOException ex) {
            System.out.println("Couln't skip bytes");
            System.out.println(ex.toString());
            throw new IllegalArgumentException("Could not skip bytes", ex);
        }

        UUID projectUID = ((BinaryReaderExImpl) r).readUuid();

        return Math.abs(projectUID.hashCode()) % numPartitions;
    }

    @Override
    public List<List<ClusterNode>> assignPartitions(AffinityFunctionContext affCtx) {
        List<List<ClusterNode>> assignments = new ArrayList<>(numPartitions);
        List<ClusterNode> nodes = affCtx.currentTopologySnapshot();

        for (int i = 0; i < numPartitions; i++) {
            assignments.add(nodes);
        }
        return assignments;
    }

    @Override
    public void removeNode(UUID nodeId) {
        //No-oP
    }
}
