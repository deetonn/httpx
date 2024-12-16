// Start time
let startTime = Date.now();

// Maximum number of concurrent requests
const MAX_CONCURRENT_REQUESTS = 1000; // Adjust based on the system's capability
const TOTAL_REQUESTS = 100000;
let completedRequests = 0;

async function fetchWeatherData() {
  // Display loading message
  document.getElementById('loading').innerText = 'Fetching data... Please wait.';

  try {
    // Function to make batches of requests
    async function makeBatch(startIndex) {
      const batchPromises = [];

      // Make requests in batches
      for (let i = startIndex; i < startIndex + MAX_CONCURRENT_REQUESTS && i < TOTAL_REQUESTS; i++) {
        batchPromises.push(fetch('/api/weather'));
      }

      // Wait for the current batch of requests to finish
      await Promise.all(batchPromises);

      completedRequests += batchPromises.length;

      // Update the progress
      updateProgress();

      // If we haven't finished all requests, make the next batch
      if (completedRequests < TOTAL_REQUESTS) {
        // Give the browser a chance to clean up
        await new Promise(resolve => setTimeout(resolve, 0));
        await makeBatch(completedRequests);
      }
    }

    // Start the first batch of requests
    await makeBatch(0);

    // End time
    const endTime = Date.now();

    // Calculate the total time taken
    const timeTaken = endTime - startTime;

    // Display the result
    document.getElementById('loading').style.display = 'none';
    document.getElementById('result').innerText = `Time taken for 10,000 requests: ${timeTaken} ms`;
  } catch (error) {
    // Handle any errors and display them
    document.getElementById('loading').style.display = 'none';
    document.getElementById('result').innerText = `An error occurred: ${error.message}`;
    document.getElementById('result').classList.add('error');
  }
}

// Update the progress bar
function updateProgress() {
  const progress = Math.min((completedRequests / TOTAL_REQUESTS) * 100, 100);
  document.getElementById('result').innerText = `Progress: ${completedRequests} / ${TOTAL_REQUESTS} requests completed (${Math.round(progress)}%)`;
}

// Execute the function
fetchWeatherData();

// Start time
startTime = Date.now();
completedRequests = 0;

async function fetchJsonData() {
  // Display loading message
  document.getElementById('loading').innerText = 'Fetching data... Please wait.';

  try {
    // Function to make batches of requests
    async function makeBatch(startIndex) {
      const batchPromises = [];

      // Make requests in batches
      for (let i = startIndex; i < startIndex + MAX_CONCURRENT_REQUESTS && i < TOTAL_REQUESTS; i++) {
        batchPromises.push(fetch('/api/json'));
      }

      // Wait for the current batch of requests to finish
      await Promise.all(batchPromises);

      completedRequests += batchPromises.length;

      // Update the progress
      updateProgress();

      // If we haven't finished all requests, make the next batch
      if (completedRequests < TOTAL_REQUESTS) {
        // Give the browser a chance to clean up
        await new Promise(resolve => setTimeout(resolve, 0));
        await makeBatch(completedRequests);
      }
    }

    // Start the first batch of requests
    await makeBatch(0);

    // End time
    const endTime = Date.now();

    // Calculate the total time taken
    const timeTaken = endTime - startTime;

    // Display the result
    document.getElementById('loading').style.display = 'none';
    document.getElementById('result').innerText = `Time taken for 10,000 requests: ${timeTaken} ms`;
  } catch (error) {
    // Handle any errors and display them
    document.getElementById('loading').style.display = 'none';
    document.getElementById('result').innerText = `An error occurred: ${error.message}`;
    document.getElementById('result').classList.add('error');
  }
}

// Execute the function
fetchJsonData();
