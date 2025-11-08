const API_URL = '/api/task';

// Загрузка задач при старте
document.addEventListener('DOMContentLoaded', loadTasks);

async function loadTasks() {
    try {
        const response = await fetch(API_URL);
        const tasks = await response.json();
        displayTasks(tasks);
    } catch (error) {
        console.error('Error loading tasks:', error);
    }
}

function displayTasks(tasks) {
    const tasksList = document.getElementById('tasksList');
    const statsText = document.getElementById('statsText');

    tasksList.innerHTML = '';

    tasks.forEach(task => {
        const taskElement = document.createElement('div');
        taskElement.className = `task-item ${task.isCompleted ? 'completed' : ''}`;
        taskElement.setAttribute('data-task-id', task.id);

        taskElement.innerHTML = `
            <span class="task-title">${task.description}</span>
            <div class="task-actions">
                <button onclick="toggleTask(${task.id})" class="toggle-btn">
                    ${task.isCompleted ? '↶' : '✓'}
                </button>
                <button onclick="enableEditMode(${task.id}, '${task.description.replace(/'/g, "\\'")}')" class="edit-btn">✏️</button>
                <button onclick="deleteTask(${task.id})" class="delete-btn">🗑️</button>
            </div>
        `;
        tasksList.appendChild(taskElement);
    });

    // Обновляем статистику
    const completed = tasks.filter(t => t.isCompleted).length;
    statsText.textContent = `Всего задач: ${tasks.length} | Выполнено: ${completed}`;
}

async function addTask() {
    const input = document.getElementById('taskInput');
    const description = input.value.trim();

    if (!description) return;

    try {
        console.log('Sending task:', description);

        const response = await fetch(API_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                title: description,       
                description: description,
                isCompleted: false,
                priority: 1,
                createAt: new Date().toISOString()
            })
        });

        console.log('Response status:', response.status);

        if (response.ok) {
            const createdTask = await response.json();
            console.log('Task created:', createdTask);
            input.value = '';
            loadTasks();
        } else {
            const errorText = await response.text();
            console.error('Error response:', errorText);
        }
    } catch (error) {
        console.error('Error adding task:', error);
    }
}

async function toggleTask(id) {
    try {
        const response = await fetch(`${API_URL}/${id}/toggle`, {
            method: 'PATCH'
        });

        if (response.ok) {
            loadTasks();
        } else {
            console.error('Error toggling task:', response.status);
        }
    } catch (error) {
        console.error('Error toggling task:', error);
    }
}

async function deleteTask(id) {
    try {
        await fetch(`${API_URL}/${id}`, {
            method: 'DELETE'
        });
        loadTasks();
    } catch (error) {
        console.error('Error deleting task:', error);
    }
}

async function editTask(id, newDescription) {
    try {
        // Сначала получаем текущую задачу
        const response = await fetch(`${API_URL}/${id}`);
        const task = await response.json();

        // Обновляем описание
        task.description = newDescription;

        // Отправляем обновленную задачу
        const updateResponse = await fetch(`${API_URL}/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(task)
        });

        if (updateResponse.ok) {
            loadTasks();
        } else {
            console.error('Error updating task:', updateResponse.status);
        }
    } catch (error) {
        console.error('Error updating task:', error);
    }
}

function enableEditMode(taskId, currentDescription) {
    const taskElement = document.querySelector(`[data-task-id="${taskId}"]`);
    const taskTitle = taskElement.querySelector('.task-title');

    // Заменяем текст на input поле
    taskTitle.innerHTML = `
        <input type="text" id="editInput-${taskId}" value="${currentDescription}" class="edit-input">
        <button onclick="saveEdit(${taskId})" class="save-btn">💾</button>
        <button onclick="cancelEdit(${taskId}, '${currentDescription}')" class="cancel-btn">❌</button>
    `;
}

function saveEdit(taskId) {
    const input = document.getElementById(`editInput-${taskId}`);
    const newDescription = input.value.trim();

    if (newDescription) {
        editTask(taskId, newDescription);
    }
}

function cancelEdit(taskId, originalDescription) {
    const taskElement = document.querySelector(`[data-task-id="${taskId}"]`);
    const taskTitle = taskElement.querySelector('.task-title');

    // Возвращаем оригинальный текст
    taskTitle.innerHTML = originalDescription;
}
async function testValidation() {
    console.log('=== Testing validation ===');

    // Тест 1: Пустые данные
    console.log('Test 1: Empty object');
    await sendTestRequest({});

    // Тест 2: Только пробелы
    console.log('Test 2: Only spaces');
    await sendTestRequest({
        title: "   ",
        description: "   "
    });

    // Тест 3: Слишком длинное описание
    console.log('Test 3: Very long description');
    await sendTestRequest({
        title: "Test",
        description: "A".repeat(600) // > 500 символов
    });
}

async function sendTestRequest(testData) {
    try {
        const response = await fetch(API_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(testData)
        });

        console.log(`Status: ${response.status}`);
        const result = await response.text();
        console.log('Response:', result);
        console.log('---');
    } catch (error) {
        console.error('Error:', error);
    }
}