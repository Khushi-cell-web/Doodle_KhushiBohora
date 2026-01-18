// Chart.js helper functions for Mood History Chart

let moodHistoryCharts = {};

window.createMoodHistoryChart = function (canvasId, dates, categoryValues, categories) {
    try {
        // Destroy existing chart if it exists
        if (moodHistoryCharts[canvasId]) {
            moodHistoryCharts[canvasId].destroy();
        }

        const ctx = document.getElementById(canvasId);
        if (!ctx) {
            console.error(`Canvas element with id '${canvasId}' not found`);
            return;
        }

        // Prepare data points with colors
        const dataPoints = categoryValues.map((value, index) => {
            let color = '#787F56'; // Default olive
            if (categories[index] === 'Positive') {
                color = '#4AE24A'; // Green for positive
            } else if (categories[index] === 'Negative') {
                color = '#E24A4A'; // Red for negative
            } else {
                color = '#787F56'; // Olive for neutral
            }

            return {
                x: dates[index],
                y: value,
                category: categories[index],
                color: color
            };
        });

        moodHistoryCharts[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: dates,
                datasets: [{
                    label: 'Mood Category',
                    data: categoryValues,
                    borderColor: '#787F56',
                    backgroundColor: 'rgba(120, 127, 86, 0.1)',
                    borderWidth: 2,
                    pointRadius: 6,
                    pointHoverRadius: 8,
                    pointBackgroundColor: dataPoints.map(p => p.color),
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    tension: 0.3,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const index = context.dataIndex;
                                const category = categories[index];
                                const date = dates[index];
                                return `${date}: ${category}`;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 2,
                        ticks: {
                            stepSize: 1,
                            callback: function(value) {
                                if (value === 2) return 'Positive';
                                if (value === 1) return 'Neutral';
                                if (value === 0) return 'Negative';
                                return '';
                            }
                        },
                        grid: {
                            color: 'rgba(120, 127, 86, 0.2)'
                        }
                    },
                    x: {
                        grid: {
                            color: 'rgba(120, 127, 86, 0.2)'
                        },
                        ticks: {
                            maxRotation: 45,
                            minRotation: 45
                        }
                    }
                }
            }
        });
    } catch (error) {
        console.error('Error creating mood history chart:', error);
    }
};

window.updateMoodHistoryChart = function (canvasId, dates, categoryValues, categories) {
    try {
        if (!moodHistoryCharts[canvasId]) {
            window.createMoodHistoryChart(canvasId, dates, categoryValues, categories);
            return;
        }

        const chart = moodHistoryCharts[canvasId];
        
        // Update chart data
        chart.data.labels = dates;
        chart.data.datasets[0].data = categoryValues;
        
        // Update point colors
        const dataPoints = categoryValues.map((value, index) => {
            let color = '#787F56';
            if (categories[index] === 'Positive') {
                color = '#4AE24A';
            } else if (categories[index] === 'Negative') {
                color = '#E24A4A';
            }
            return color;
        });
        chart.data.datasets[0].pointBackgroundColor = dataPoints;
        
        // Update tooltip callback
        chart.options.plugins.tooltip.callbacks.label = function(context) {
            const index = context.dataIndex;
            const category = categories[index];
            const date = dates[index];
            return `${date}: ${category}`;
        };
        
        chart.update();
    } catch (error) {
        console.error('Error updating mood history chart:', error);
        // Fallback to recreate
        window.createMoodHistoryChart(canvasId, dates, categoryValues, categories);
    }
};

window.destroyChart = function (canvasId) {
    try {
        if (moodHistoryCharts[canvasId]) {
            moodHistoryCharts[canvasId].destroy();
            delete moodHistoryCharts[canvasId];
        }
    } catch (error) {
        console.error('Error destroying chart:', error);
    }
};
