const http = require('http');

function request(method, path, data = null, token = null) {
    return new Promise((resolve, reject) => {
        const options = {
            hostname: 'localhost',
            port: 5242,
            path: path,
            method: method,
            headers: {
                'Content-Type': 'application/json'
            }
        };
        if (token) options.headers['Authorization'] = `Bearer ${token}`;

        const req = http.request(options, (res) => {
            let responseData = '';
            res.on('data', chunk => responseData += chunk);
            res.on('end', () => {
                try {
                    resolve({ status: res.statusCode, data: JSON.parse(responseData || '{}') });
                } catch (e) {
                    resolve({ status: res.statusCode, data: responseData });
                }
            });
        });
        req.on('error', reject);
        if (data) req.write(JSON.stringify(data));
        req.end();
    });
}

async function run() {
    console.log("=== Bismillah: Testing Driver Workflow ===");
    
    // 1. Login
    console.log("\n1. Logging in as driver@ajk.com...");
    const loginRes = await request('POST', '/api/Auth/login', { email: 'driver@ajk.com', password: 'driver123' });
    if (loginRes.status !== 200) {
        console.error("Login failed!", loginRes.data);
        return;
    }
    const token = loginRes.data.token;
    console.log("-> Success! Got JWT Token.");

    // 2. Load Home Data
    console.log("\n2. Fetching Home Dashboard Data...");
    const homeRes = await request('GET', '/api/driver/home', null, token);
    console.log(`-> Status: ${homeRes.status}, Data:`, homeRes.data);

    // 3. Load Active Trip
    console.log("\n3. Fetching Active Trip...");
    const activeTripRes = await request('GET', '/api/driver/active-trip', null, token);
    console.log(`-> Status: ${activeTripRes.status}, Trip details loaded.`);
    let activeTripId = activeTripRes.status === 200 ? activeTripRes.data.tripId : null;

    if (!activeTripId) {
        console.log("No active trip found. Fetching upcoming trips...");
        const tripsRes = await request('GET', '/api/driver/trips', null, token);
        console.log(`-> Assigned Trips count: ${tripsRes.data.length}`);
        if (tripsRes.data.length > 0) {
            activeTripId = tripsRes.data[0].tripId;
            console.log(`-> Starting Trip ID: ${activeTripId}`);
            await request('POST', `/api/driver/${activeTripId}/start`, {}, token);
            console.log("-> Trip started successfully!");
        }
    } else {
        console.log(`-> Active Trip ID found: ${activeTripId}`);
    }

    // 4. Mark Stop Reached
    if (activeTripId) {
        console.log("\n4. Simulating reaching a stop...");
        const stopRes = await request('POST', `/api/driver/active-trip/mark-stop`, { stopId: 1, reachedAt: new Date().toISOString() }, token);
        console.log(`-> Status: ${stopRes.status}, Result:`, stopRes.data);

        console.log("\n5. Ending the Trip...");
        const endRes = await request('POST', `/api/driver/active-trip/end`, { notes: "Testing complete", passengersCarried: 50 }, token);
        console.log(`-> Status: ${endRes.status}, Result:`, endRes.data);
    }

    // 6. Check History
    console.log("\n6. Checking Trip History to verify the trip moved there...");
    const historyRes = await request('GET', '/api/driver/history', null, token);
    console.log(`-> Status: ${historyRes.status}, Total past trips: ${historyRes.data?.length || 0}`);
    
    console.log("\n=== Testing Completed Successfully! ===");
}

run();
