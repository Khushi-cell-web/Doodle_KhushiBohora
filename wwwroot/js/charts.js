// Chart.js helper functions for Mood History Chart and Trends Charts

let moodHistoryCharts = {};
let trendsCharts = {};
let dashboardCharts = {};

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
        if (trendsCharts[canvasId]) {
            trendsCharts[canvasId].destroy();
            delete trendsCharts[canvasId];
        }
        if (dashboardCharts[canvasId]) {
            dashboardCharts[canvasId].destroy();
            delete dashboardCharts[canvasId];
        }
    } catch (error) {
        console.error('Error destroying chart:', error);
    }
};

// Mood Distribution Chart (Doughnut Chart with Light Colors)
window.createMoodDistributionChart = function (canvasId, moodNames, moodCounts) {
    try {
        if (dashboardCharts[canvasId]) {
            dashboardCharts[canvasId].destroy();
        }

        const ctx = document.getElementById(canvasId);
        if (!ctx) {
            console.error(`Canvas element with id '${canvasId}' not found`);
            return;
        }

        // Generate light pastel colors for each mood
        const lightColors = [
            'rgba(120, 127, 86, 0.6)',   // Light olive
            'rgba(156, 175, 136, 0.6)',  // Light sage green
            'rgba(255, 193, 7, 0.6)',    // Light yellow
            'rgba(255, 182, 193, 0.6)',  // Light pink
            'rgba(173, 216, 230, 0.6)',  // Light blue
            'rgba(221, 160, 221, 0.6)',  // Light plum
            'rgba(255, 218, 185, 0.6)',  // Light peach
            'rgba(144, 238, 144, 0.6)',  // Light green
            'rgba(255, 228, 196, 0.6)',  // Light bisque
            'rgba(176, 224, 230, 0.6)',  // Light powder blue
            'rgba(255, 182, 193, 0.6)',  // Light pink
            'rgba(230, 230, 250, 0.6)',  // Light lavender
            'rgba(255, 228, 225, 0.6)',  // Light misty rose
            'rgba(240, 248, 255, 0.6)',  // Light alice blue
            'rgba(245, 245, 220, 0.6)'   // Light beige
        ];

        const backgroundColors = moodNames.map((_, index) => {
            return lightColors[index % lightColors.length];
        });

        const borderColors = moodNames.map((_, index) => {
            // Slightly darker borders for definition
            const baseColor = lightColors[index % lightColors.length];
            return baseColor.replace('0.6', '0.8');
        });

        // Create labels with counts for legend
        const labelsWithCounts = moodNames.map((name, index) => {
            return `${name} (${moodCounts[index]})`;
        });

        dashboardCharts[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labelsWithCounts, // Labels include counts
                datasets: [{
                    label: 'Mood Count',
                    data: moodCounts,
                    backgroundColor: backgroundColors,
                    borderColor: borderColors,
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '60%', // Creates the doughnut hole
                plugins: {
                    legend: {
                        display: true,
                        position: 'right',
                        labels: {
                            padding: 10,
                            usePointStyle: true,
                            font: {
                                size: 11
                            },
                            color: '#333'
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const label = moodNames[context.dataIndex] || '';
                                const value = context.parsed || 0;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((value / total) * 100).toFixed(1);
                                return `${label}: ${value} time${value !== 1 ? 's' : ''} (${percentage}%)`;
                            }
                        }
                    }
                },
                // Custom plugin to draw count numbers on chart segments
                onHover: function(event, activeElements) {
                    ctx.canvas.style.cursor = activeElements.length > 0 ? 'pointer' : 'default';
                }
            },
            plugins: [{
                id: 'moodCountLabels',
                afterDraw: function(chart) {
                    const ctx = chart.ctx;
                    const data = chart.data;
                    const meta = chart.getDatasetMeta(0);
                    
                    if (!meta || !meta.data) return;
                    
                    meta.data.forEach((element, index) => {
                        const value = data.datasets[0].data[index];
                        if (value > 0 && element) {
                            try {
                                const centerX = chart.chartArea.left + (chart.chartArea.right - chart.chartArea.left) / 2;
                                const centerY = chart.chartArea.top + (chart.chartArea.bottom - chart.chartArea.top) / 2;
                                
                                // Get the angle of the segment (midpoint)
                                const startAngle = element.startAngle;
                                const endAngle = element.endAngle;
                                const angle = (startAngle + endAngle) / 2;
                                
                                // Calculate position on the segment (midpoint between inner and outer radius)
                                const innerRadius = element.innerRadius || 0;
                                const outerRadius = element.outerRadius || 0;
                                const radius = innerRadius + (outerRadius - innerRadius) / 2;
                                
                                const x = centerX + Math.cos(angle) * radius;
                                const y = centerY + Math.sin(angle) * radius;
                                
                                ctx.save();
                                ctx.fillStyle = '#333';
                                ctx.font = 'bold 11px Arial';
                                ctx.textAlign = 'center';
                                ctx.textBaseline = 'middle';
                                ctx.fillText(value.toString(), x, y);
                                ctx.restore();
                            } catch (e) {
                                console.error('Error drawing label:', e);
                            }
                        }
                    });
                }
            }]
        });
    } catch (error) {
        console.error('Error creating mood distribution chart:', error);
    }
};

window.updateMoodDistributionChart = function (canvasId, moodNames, moodCounts) {
    try {
        if (!dashboardCharts[canvasId]) {
            window.createMoodDistributionChart(canvasId, moodNames, moodCounts);
            return;
        }

        const chart = dashboardCharts[canvasId];
        
        // Generate light pastel colors for each mood
        const lightColors = [
            'rgba(120, 127, 86, 0.6)',   // Light olive
            'rgba(156, 175, 136, 0.6)',  // Light sage green
            'rgba(255, 193, 7, 0.6)',    // Light yellow
            'rgba(255, 182, 193, 0.6)',  // Light pink
            'rgba(173, 216, 230, 0.6)',  // Light blue
            'rgba(221, 160, 221, 0.6)',  // Light plum
            'rgba(255, 218, 185, 0.6)',  // Light peach
            'rgba(144, 238, 144, 0.6)',  // Light green
            'rgba(255, 228, 196, 0.6)',  // Light bisque
            'rgba(176, 224, 230, 0.6)',  // Light powder blue
            'rgba(255, 182, 193, 0.6)',  // Light pink
            'rgba(230, 230, 250, 0.6)',  // Light lavender
            'rgba(255, 228, 225, 0.6)',  // Light misty rose
            'rgba(240, 248, 255, 0.6)',  // Light alice blue
            'rgba(245, 245, 220, 0.6)'   // Light beige
        ];

        const backgroundColors = moodNames.map((_, index) => {
            return lightColors[index % lightColors.length];
        });

        const borderColors = moodNames.map((_, index) => {
            return lightColors[index % lightColors.length].replace('0.6', '0.8');
        });

        // Create labels with counts for legend
        const labelsWithCounts = moodNames.map((name, index) => {
            return `${name} (${moodCounts[index]})`;
        });

        chart.data.labels = labelsWithCounts;
        chart.data.datasets[0].data = moodCounts;
        chart.data.datasets[0].backgroundColor = backgroundColors;
        chart.data.datasets[0].borderColor = borderColors;
        chart.update();
    } catch (error) {
        console.error('Error updating mood distribution chart:', error);
        window.createMoodDistributionChart(canvasId, moodNames, moodCounts);
    }
};

// Writing Frequency Chart (Bar Chart)
window.createWritingFrequencyChart = function (canvasId, dates, counts) {
    try {
        if (trendsCharts[canvasId]) {
            trendsCharts[canvasId].destroy();
        }

        const ctx = document.getElementById(canvasId);
        if (!ctx) {
            console.error(`Canvas element with id '${canvasId}' not found`);
            return;
        }

        trendsCharts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: dates,
                datasets: [{
                    label: 'Entries',
                    data: counts,
                    backgroundColor: 'rgba(120, 127, 86, 0.6)',
                    borderColor: '#787F56',
                    borderWidth: 2
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
                                return `Entries: ${context.parsed.y}`;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1
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
        console.error('Error creating writing frequency chart:', error);
    }
};

window.updateWritingFrequencyChart = function (canvasId, dates, counts) {
    try {
        if (!trendsCharts[canvasId]) {
            window.createWritingFrequencyChart(canvasId, dates, counts);
            return;
        }

        const chart = trendsCharts[canvasId];
        chart.data.labels = dates;
        chart.data.datasets[0].data = counts;
        chart.update();
    } catch (error) {
        console.error('Error updating writing frequency chart:', error);
        window.createWritingFrequencyChart(canvasId, dates, counts);
    }
};

// Word Count Trend Chart (Line Chart)
window.createWordCountTrendChart = function (canvasId, dates, wordCounts) {
    try {
        if (trendsCharts[canvasId]) {
            trendsCharts[canvasId].destroy();
        }

        const ctx = document.getElementById(canvasId);
        if (!ctx) {
            console.error(`Canvas element with id '${canvasId}' not found`);
            return;
        }

        trendsCharts[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: dates,
                datasets: [{
                    label: 'Average Word Count',
                    data: wordCounts,
                    borderColor: '#787F56',
                    backgroundColor: 'rgba(120, 127, 86, 0.1)',
                    borderWidth: 2,
                    pointRadius: 4,
                    pointHoverRadius: 6,
                    pointBackgroundColor: '#787F56',
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
                                return `Avg Words: ${Math.round(context.parsed.y)}`;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
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
        console.error('Error creating word count trend chart:', error);
    }
};

window.updateWordCountTrendChart = function (canvasId, dates, wordCounts) {
    try {
        if (!trendsCharts[canvasId]) {
            window.createWordCountTrendChart(canvasId, dates, wordCounts);
            return;
        }

        const chart = trendsCharts[canvasId];
        chart.data.labels = dates;
        chart.data.datasets[0].data = wordCounts;
        chart.update();
    } catch (error) {
        console.error('Error updating word count trend chart:', error);
        window.createWordCountTrendChart(canvasId, dates, wordCounts);
    }
};

// Mood Balance Chart (Stacked Bar Chart)
window.createMoodBalanceChart = function (canvasId, dates, positive, neutral, negative) {
    try {
        if (trendsCharts[canvasId]) {
            trendsCharts[canvasId].destroy();
        }

        const ctx = document.getElementById(canvasId);
        if (!ctx) {
            console.error(`Canvas element with id '${canvasId}' not found`);
            return;
        }

        trendsCharts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: dates,
                datasets: [
                    {
                        label: 'Positive',
                        data: positive,
                        backgroundColor: 'rgba(74, 226, 74, 0.7)',
                        borderColor: '#4AE24A',
                        borderWidth: 1
                    },
                    {
                        label: 'Neutral',
                        data: neutral,
                        backgroundColor: 'rgba(120, 127, 86, 0.7)',
                        borderColor: '#787F56',
                        borderWidth: 1
                    },
                    {
                        label: 'Negative',
                        data: negative,
                        backgroundColor: 'rgba(226, 74, 74, 0.7)',
                        borderColor: '#E24A4A',
                        borderWidth: 1
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'top'
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return `${context.dataset.label}: ${context.parsed.y}`;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        stacked: true,
                        grid: {
                            color: 'rgba(120, 127, 86, 0.2)'
                        },
                        ticks: {
                            maxRotation: 45,
                            minRotation: 45
                        }
                    },
                    y: {
                        stacked: true,
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1
                        },
                        grid: {
                            color: 'rgba(120, 127, 86, 0.2)'
                        }
                    }
                }
            }
        });
    } catch (error) {
        console.error('Error creating mood balance chart:', error);
    }
};

window.updateMoodBalanceChart = function (canvasId, dates, positive, neutral, negative) {
    try {
        if (!trendsCharts[canvasId]) {
            window.createMoodBalanceChart(canvasId, dates, positive, neutral, negative);
            return;
        }

        const chart = trendsCharts[canvasId];
        chart.data.labels = dates;
        chart.data.datasets[0].data = positive;
        chart.data.datasets[1].data = neutral;
        chart.data.datasets[2].data = negative;
        chart.update();
    } catch (error) {
        console.error('Error updating mood balance chart:', error);
        window.createMoodBalanceChart(canvasId, dates, positive, neutral, negative);
    }
};
